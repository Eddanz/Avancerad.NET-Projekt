using AutoMapper;
using Avancerad.NET_Projekt.Services;
using Avancerad.NET_Projekt_ClassLibrary.Models;
using Avancerad.NET_Projekt.Methods;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Avancerad.NET_Projekt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerRepo _customerRepo;
        private readonly IMapper _mapper;

        public CustomerController(ICustomerRepo customerRepo, IMapper mapper)
        {
            _customerRepo = customerRepo;
            _mapper = mapper;
        }

        [HttpGet, Authorize]
        public async Task<ActionResult<IEnumerable<CustomerDTO>>> GetAllCustomers()
        {
            try
            {
                var customer = await _customerRepo.GetAllAsync();
                var customerDTO = _mapper.Map<IEnumerable<CustomerDTO>>(customer);

                return Ok(customerDTO);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error to retrive data from database..");
            }
        }

        [HttpGet("appointments/{id:int}")]
        public async Task<ActionResult<object>> GetCustomerAppointments(int id)
        {
            try
            {
                var customer = await _customerRepo.GetCustomerWithAppointments(id);

                if (customer == null)
                {
                    return NotFound($"Customer with ID {id} does not exist.");
                }

                // Filter out appointments where IsDeleted is false
                var appointments = customer.Appointments.Where(a => !a.IsDeleted).ToList();

                // Check if the customer has any non-deleted appointments
                if (appointments.Count == 0)
                {
                    return NotFound($"Customer with ID {id} does not have any active appointments.");
                }

                var customerDTO = _mapper.Map<CustomerDTO>(customer);
                var appointmentsDTO = _mapper.Map<List<AppointmentDTO>>(appointments);

                // Create an anonymous object containing customer info and appointments
                var response = new
                {
                    Customer = customerDTO,
                    Appointments = appointmentsDTO
                };

                return Ok(response);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error retrieving data from the database.");
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<CustomerDTO>> GetCustomer(int id)
        {
            try
            {
                var customer = await _customerRepo.GetByIdAsync(id);

                if (customer == null)
                {
                    return NotFound($"Customer with ID {id} does not exist.");
                }

                var customerDTO = _mapper.Map<CustomerDTO>(customer);
                return Ok(customerDTO);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error retrieving data from database.");
            }
        }

        [HttpPost]
        public async Task<ActionResult<CustomerDTO>> CreateCustomer(CustomerDTO newCustomerDTO)
        {
            try
            {
                if (newCustomerDTO == null)
                {
                    return BadRequest();
                }

                // Map CustomerDTO to Customer
                var newCustomer = _mapper.Map<Customer>(newCustomerDTO);

                // Create the customer using the service
                var createdCustomer = await _customerRepo.CreateAsync(newCustomer);

                // Map the created Customer back to CustomerDTO
                var createdCustomerDTO = _mapper.Map<CustomerDTO>(createdCustomer);

                return CreatedAtAction(nameof(GetCustomer),
                    new { id = createdCustomer.CustomerID },
                    createdCustomerDTO);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error creating data in the database.");
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<CustomerDTO>> DeleteCustomer(int id)
        {
            try
            {
                // Fetch the customer to delete
                var customerToDelete = await _customerRepo.GetByIdAsync(id);

                // Check if the customer exists
                if (customerToDelete == null)
                {
                    return NotFound($"Customer with ID {id} does not exist and therefore cannot be deleted.");
                }

                // Map the customer to CustomerDTO
                var customerDTOToReturn = _mapper.Map<CustomerDTO>(customerToDelete);

                // Perform the deletion
                await _customerRepo.DeleteAsync(id);

                // Return the CustomerDTO with a status code of OK (HTTP 200)
                return Ok(customerDTOToReturn);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error deleting data from the database.");
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> UpdateCustomer(int id, CustomerDTO customerDTO)
        {
            try
            {
                // Check if CustomerDTO is provided
                if (customerDTO == null)
                {
                    return BadRequest();
                }

                // Get the existing customer to update
                var existingCustomer = await _customerRepo.GetByIdAsync(id);
                if (existingCustomer == null)
                {
                    return NotFound($"Customer with ID {id} does not exist.");
                }

                // Map the CustomerDTO to Customer and set the ID
                var updatedCustomer = _mapper.Map<Customer>(customerDTO);
                updatedCustomer.CustomerID = id;

                // Perform the update using the service
                await _customerRepo.UpdateAsync(updatedCustomer);
                return NoContent(); // Return 204 No Content after successful update
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error updating data in the database.");
            }
        }

        [HttpGet("customers/{customerId:int}/appointments/week/{year:int}/{weekNumber:int}")]
        public async Task<ActionResult<int>> GetCustomerAppointmentsInWeek(int customerId, int year, int weekNumber)
        {
            try
            {
                // Retrieve the count of appointments for the specified customer within the specified week
                int appointmentsCount = await _customerRepo.GetCustomerAppointmentsWeek(customerId, year, weekNumber);

                return Ok(appointmentsCount);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving customer appointments for week.");
            }
        }
    }
}
