using AutoMapper;
using Avancerad.NET_Projekt.Services;
using Avancerad.NET_Projekt_ClassLibrary.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Avancerad.NET_Projekt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentRepo _appointmentRepo;
        private readonly IHistoryRepo _historyRepo;
        private readonly IMapper _mapper;

        public AppointmentController(IAppointmentRepo appointmentRepo, IHistoryRepo historyRepo ,IMapper mapper)
        {
            _appointmentRepo = appointmentRepo;
            _historyRepo = historyRepo;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppointmentDTO>>> GetAllAppointments()
        {
            try
            {
                var appointment = await _appointmentRepo.GetAllAsync();

                // Filter out appointments where IsDeleted is false
                appointment = appointment.Where(a => !a.IsDeleted);

                var appointmentDTO = _mapper.Map<IEnumerable<AppointmentDTO>>(appointment);

                return Ok(appointmentDTO);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error to retrive data from database..");
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<AppointmentDTO>> GetAppointment(int id)
        {
            try
            {
                var appointment = await _appointmentRepo.GetByIdAsync(id);

                if (appointment == null || appointment.IsDeleted)
                {
                    return NotFound($"Appointment with ID {id} does not exist or is flagged as deleted.");
                }

                var appointmentDTO = _mapper.Map<AppointmentDTO>(appointment);
                return Ok(appointmentDTO);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error retrieving data from database.");
            }
        }

        [HttpGet("customers-with-appointments-in-current-week")]
        public async Task<ActionResult<IEnumerable<CustomerDTO>>> GetCustomersWithAppointmentsInCurrentWeek()
        {
            try
            {
                var customers = await _appointmentRepo.GetCustomersWithAppointmentsInCurrentWeek();

                // Filter out appointments where IsDeleted is false
                customers = customers.Where(c => c.Appointments.Any(a => !a.IsDeleted));

                // Map customers to CustomerDTOs
                var customerDTOs = _mapper.Map<IEnumerable<CustomerDTO>>(customers);

                return Ok(customerDTOs);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error retrieving data from the database.");
            }
        }

        [HttpPost]
        public async Task<ActionResult<AppointmentDTO>> CreateAppointment(AppointmentDTO newAppointmentDTO)
        {
            try
            {
                if (newAppointmentDTO == null)
                {
                    return BadRequest();
                }

                var newAppointment = _mapper.Map<Appointment>(newAppointmentDTO);

                var createdAppointment = await _appointmentRepo.CreateAsync(newAppointment);

                var createdAppointmentDTO = _mapper.Map<AppointmentDTO>(createdAppointment);

                // Add a change to the appointment history
                var historyEntry = new History
                {
                    AppointmentID = createdAppointment.AppointmentID,
                    ChangeDate = DateTime.UtcNow,
                    ChangeType = "Added",
                    ChangeDescription = $"Appointment created at {DateTime.Now}"
                };
                await _historyRepo.CreateAsync(historyEntry);

                return CreatedAtAction(nameof(GetAppointment),
                    new { id = createdAppointment.AppointmentID },
                    createdAppointmentDTO);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error creating data in the database.");
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<AppointmentDTO>> DeleteAppointment(int id)
        {
            try
            {
                var appointmentToDelete = await _appointmentRepo.GetByIdAsync(id);

                if (appointmentToDelete == null)
                {
                    return NotFound($"Appointment with ID {id} does not exist and therefore cannot be deleted.");
                }

                // Mark the appointment as deleted
                appointmentToDelete.IsDeleted = true;

                // Update the appointment in the database
                await _appointmentRepo.UpdateAsync(appointmentToDelete);

                var appointmentDTOToReturn = _mapper.Map<AppointmentDTO>(appointmentToDelete);

                // Add a change to the appointment history indicating that the appointment has been deleted
                var historyEntry = new History
                {
                    AppointmentID = id, // Assuming AppointmentID is string in History
                    ChangeDate = DateTime.UtcNow,
                    ChangeType = "Deleted",
                    ChangeDescription = $"Appointment deleted at {DateTime.Now}"
                };
                await _historyRepo.CreateAsync(historyEntry);

                return Ok(appointmentDTOToReturn);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error deleting data from the database.");
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> UpdateAppointment(int id, AppointmentDTO appointmentDTO)
        {
            try
            {
                // Check if CustomerDTO is provided
                if (appointmentDTO == null)
                {
                    return BadRequest();
                }

                // Get the existing customer to update
                var existingAppointment = await _appointmentRepo.GetByIdAsync(id);
                if (existingAppointment == null)
                {
                    return NotFound($"Appointment with ID {id} does not exist.");
                }

                var originalAttendDate = existingAppointment.AttendDate;

                // Map the CustomerDTO to Customer and set the ID
                var updatedAppointment = _mapper.Map<Appointment>(appointmentDTO);
                updatedAppointment.AppointmentID = id;

                // Perform the update using the service
                await _appointmentRepo.UpdateAsync(updatedAppointment);

                // Spara ändringshistoriken
                var history = new History
                {
                    AppointmentID = id,
                    ChangeDate = DateTime.UtcNow,
                    ChangeType = "Changed",
                    ChangeDescription = $"Appointment changed from {originalAttendDate} to {updatedAppointment.AttendDate} at {DateTime.Now}"
                };
                await _historyRepo.CreateAsync(history);

                return NoContent(); // Return 204 No Content after successful update
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error updating data in the database.");
            }
        }
    }
}
