using AutoMapper;
using Avancerad.NET_Projekt.Services;
using Avancerad.NET_Projekt_ClassLibrary.Models;
using Avancerad.NET_Projekt.Methods;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Numerics;

namespace Avancerad.NET_Projekt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ICustomerRepo _customerRepo;
        private readonly IMapper _mapper;

        public CustomerController(ICustomerRepo customerRepo, IMapper mapper, UserManager<IdentityUser> userManager)
        {
            _customerRepo = customerRepo;
            _mapper = mapper;
            _userManager = userManager;
        }

        [HttpGet, Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminCompanyPolicy")]
        public async Task<ActionResult<IEnumerable<CustomerDTO>>> GetAllCustomers()
        {
            try
            {
                var customers = await _customerRepo.GetAllAsync();

                var customerDTO = _mapper.Map<IEnumerable<CustomerDTO>>(customers);

                return Ok(customerDTO);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error to retrive data from database..");
            }
        }

        [HttpGet("sort-or-filter"), Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminCompanyPolicy")]
        public async Task<ActionResult<IEnumerable<CustomerDTO>>> GetAllCustomersQuery(string sortBy = null, string filterBy = null)
        {
            try
            {
                var customers = await _customerRepo.GetAllAsync();

                // Filtrera kunder baserat på filterBy
                if (!string.IsNullOrEmpty(filterBy))
                {
                    customers = customers.Where(c => c.FirstName.ToLower().Contains(filterBy.ToLower()));
                }

                // Sortera kunder baserat på sortBy
                if (!string.IsNullOrEmpty(sortBy))
                {
                    switch (sortBy.ToLower())
                    {
                        case "firstname":
                            customers = customers.OrderBy(c => c.FirstName);
                            break;
                        case "lastname":
                            customers = customers.OrderBy(c => c.LastName);
                            break;
                        // Lägg till fler sorteringsalternativ här
                        default:
                            break;
                    }
                }

                var customerDTOs = _mapper.Map<IEnumerable<CustomerDTO>>(customers);

                return Ok(customerDTOs);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Fel vid hämtning av data från databasen..");
            }
        }



        [HttpGet("appointments/{id:int}"), Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminCompanyPolicy")]
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
                var appointments = customer.Appointments/*.Where(a => !a.IsDeleted)*/.ToList();

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

        [HttpGet("{id:int}"), Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminCompanyPolicy")]
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
        [Route("account/create/customer")]
        public async Task<IActionResult> CreateCompany(string email, string password, string firstName, string lastName, string phone)
        {
            IdentityUser User = await _userManager.FindByEmailAsync(email);
            if (User != null)
                return BadRequest(false);

            IdentityUser user = new()
            {
                UserName = email,
                PasswordHash = password,
                Email = email,
            };

            IdentityResult result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
                return BadRequest(false);

            Claim[] userClaims =
                [
                    new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, "company")
                ];
            await _userManager.AddClaimsAsync(user, userClaims);

            var customer = new Customer
            {
                IdentityUser = user,
                FirstName = firstName,
                LastName = lastName,
                Phone = phone,
                Email = email,
            };
            await _customerRepo.CreateAsync(customer);

            return Ok(true);
        }

        [HttpDelete("{id:int}"), Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminCompanyCustomerPolicy")]
        public async Task<ActionResult<CustomerDTO>> DeleteCustomer(int id, [FromServices] UserManager<IdentityUser> userManager)
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

                var email = User.FindFirstValue(ClaimTypes.Email);
                var loggedInUser = await userManager.FindByEmailAsync(email);

                // Check if the logged-in user exists
                if (loggedInUser == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        "Error retrieving the logged-in user.");
                }

                // Check if the logged-in user is trying to delete their own account or if they have the admin or company role
                if (loggedInUser.Id != customerToDelete.IdentityUserId && User != null && (!User.IsInRole("admin") && !User.IsInRole("company")))
                {
                    return Forbid();
                }

                // Map the customer to CustomerDTO
                var customerDTOToReturn = _mapper.Map<CustomerDTO>(customerToDelete);

                // Perform the deletion
                await _customerRepo.DeleteAsync(id);

                // Delete the associated IdentityUser
                var identityUserToDelete = await userManager.FindByIdAsync(customerToDelete.IdentityUserId);
                if (identityUserToDelete != null)
                {
                    var identityResult = await userManager.DeleteAsync(identityUserToDelete);
                    if (!identityResult.Succeeded)
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError,
                            "Error deleting associated IdentityUser from the database.");
                    }
                }

                // Return the CustomerDTO with a status code of OK (HTTP 200)
                return Ok(customerDTOToReturn);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error deleting data from the database. {ex.Message}");
            }
        }

        [HttpPut("{id:int}"), Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminCompanyCustomerPolicy")]
        public async Task<ActionResult> UpdateCustomer(int id, CustomerUpdateDTO customerDTO, [FromServices] UserManager<IdentityUser> userManager)
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

                var email = User.FindFirstValue(ClaimTypes.Email);
                var loggedInUser = await userManager.FindByEmailAsync(email);

                // Check if the logged-in user exists
                if (loggedInUser == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        "Error retrieving the logged-in user.");
                }

                // Check if the logged-in user is trying to update their own account or if they have the admin or company role
                if (loggedInUser.Id != existingCustomer.IdentityUserId && User != null && (!User.IsInRole("admin") && !User.IsInRole("company")))
                {
                    return Forbid();
                }

                // Map the CustomerDTO to Customer and set the ID
                var updatedCustomer = _mapper.Map<Customer>(customerDTO);
                updatedCustomer.CustomerID = id;

                // Perform the update using the service
                await _customerRepo.UpdateAsync(updatedCustomer);
                return NoContent(); // Return 204 No Content after successful update
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error updating data in the database. {ex.Message}");
            }
        }

        [HttpGet("customers/{customerId:int}/appointments/week/{year:int}/{weekNumber:int}"), 
            Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminCompanyPolicy")]
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
