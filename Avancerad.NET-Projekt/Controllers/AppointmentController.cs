using AutoMapper;
using Avancerad.NET_Projekt.Services;
using Avancerad.NET_Projekt_ClassLibrary.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Avancerad.NET_Projekt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentRepo _appointmentRepo;
        private readonly ICustomerRepo _customerRepo;
        private readonly IMapper _mapper;

        public AppointmentController(IAppointmentRepo appointmentRepo, ICustomerRepo customerRepo ,IMapper mapper)
        {
            _appointmentRepo = appointmentRepo;
            _customerRepo = customerRepo;
            _mapper = mapper;
        }

        [HttpGet, Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminCompanyPolicy")]
        public async Task<ActionResult<IEnumerable<AppointmentDTO>>> GetAllAppointments()
        {
            try
            {
                var appointment = await _appointmentRepo.GetAllAsync();

                var appointmentDTO = _mapper.Map<IEnumerable<AppointmentDTO>>(appointment);

                return Ok(appointmentDTO);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error to retrive data from database..");
            }
        }

        [HttpGet("{id:int}"), Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminCompanyPolicy")]
        public async Task<ActionResult<AppointmentDTO>> GetAppointment(int id)
        {
            try
            {
                var appointment = await _appointmentRepo.GetByIdAsync(id);

                if (appointment == null)
                {
                    return NotFound($"Appointment with ID {id} does not exist");
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

        [HttpGet("customers-with-appointments-in-current-week"), Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminCompanyPolicy")]
        public async Task<ActionResult<IEnumerable<CustomerDTO>>> GetCustomersWithAppointmentsInCurrentWeek()
        {
            try
            {
                var customers = await _appointmentRepo.GetCustomersWithAppointmentsInCurrentWeek();

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

        [HttpPost, Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminCompanyCustomerPolicy")]
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

        [HttpDelete("{id:int}"), Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminCompanyCustomerPolicy")]
        public async Task<ActionResult<AppointmentDTO>> DeleteAppointment(int id, [FromServices] UserManager<IdentityUser> userManager)
        {
            try
            {
                var appointmentToDelete = await _appointmentRepo.GetByIdAsync(id);

                if (appointmentToDelete == null)
                {
                    return NotFound($"Appointment with ID {id} does not exist and therefore cannot be deleted.");
                }

                var email = User.FindFirstValue(ClaimTypes.Email);
                var loggedInUser = await userManager.FindByEmailAsync(email);

                // Check if the logged-in user exists
                if (loggedInUser == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        "Error retrieving the logged-in user.");
                }

                // Get the customer associated with the appointment
                var customer = await _customerRepo.GetByIdAsync(appointmentToDelete.CustomerID);

                // Check if the logged-in user is trying to delete their own appointment or if they have the admin or company role
                if (loggedInUser.Id != customer.IdentityUserId && !User.IsInRole("admin") && !User.IsInRole("company"))
                {
                    return Forbid();
                }

                // Delete the appointment
                await _appointmentRepo.DeleteAsync(id);

                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error deleting data from the database.");
            }
        }

        [HttpPut("{id:int}"), Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminCompanyCustomerPolicy")]
        public async Task<ActionResult> UpdateAppointment(int id, AppointmentDTO appointmentDTO, [FromServices] UserManager<IdentityUser> userManager)
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

                var email = User.FindFirstValue(ClaimTypes.Email);
                var loggedInUser = await userManager.FindByEmailAsync(email);

                // Check if the logged-in user exists
                if (loggedInUser == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        "Error retrieving the logged-in user.");
                }

                // Get the customer associated with the appointment
                var customer = await _customerRepo.GetByIdAsync(existingAppointment.CustomerID);

                // Check if the logged-in user is trying to update their own account or if they have the admin or company role
                if (loggedInUser.Id != customer.IdentityUserId && User != null && (!User.IsInRole("admin") && !User.IsInRole("company")))
                {
                    return Forbid();
                }

                var originalAttendDate = existingAppointment.AttendDate;

                // Map the CustomerDTO to Customer and set the ID
                var updatedAppointment = _mapper.Map<Appointment>(appointmentDTO);
                updatedAppointment.AppointmentID = id;

                // Perform the update using the service
                await _appointmentRepo.UpdateAsync(updatedAppointment);

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
