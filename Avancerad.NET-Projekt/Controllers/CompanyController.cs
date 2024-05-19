using AutoMapper;
using Avancerad.NET_Projekt.Methods;
using Avancerad.NET_Projekt.Services;
using Avancerad.NET_Projekt_ClassLibrary.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Avancerad.NET_Projekt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ICompanyRepo _companyRepo;
        private readonly IMapper _mapper;

        public CompanyController(ICompanyRepo companyRepo, IMapper mapper, UserManager<IdentityUser> userManager)
        {
            _companyRepo = companyRepo;
            _mapper = mapper;
            _userManager = userManager;
        }

        [HttpGet, Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminCompanyPolicy")]
        public async Task<ActionResult<IEnumerable<CompanyDTO>>> GetAllCompanys()
        {
            try
            {
                var company = await _companyRepo.GetAllAsync();
                var companyDTO = _mapper.Map<IEnumerable<CompanyDTO>>(company);

                return Ok(companyDTO);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error to retrive data from database..");
            }
        }

        [HttpGet("{id:int}"), Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminCompanyPolicy")]
        public async Task<ActionResult<CompanyDTO>> GetCompany(int id)
        {
            try
            {
                var company = await _companyRepo.GetByIdAsync(id);

                if (company == null)
                {
                    return NotFound($"Company with ID {id} does not exist.");
                }

                var companyDTO = _mapper.Map<CompanyDTO>(company);
                return Ok(companyDTO);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error retrieving data from database.");
            }
        }

        [HttpPost, Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminCompanyPolicy")]
        [Route("account/create/company")]
        public async Task<IActionResult> CreateCompany(string email, string password, string companyName)
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

            var company = new Company
            {
                IdentityUser = user,
                CompanyName = companyName,
                Email = email,
            };
            await _companyRepo.CreateAsync(company);

            return Ok(true);
        }


        [HttpDelete("{id:int}"), Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminCompanyPolicy")]
        public async Task<ActionResult<CompanyDTO>> DeleteCompany(int id, [FromServices] UserManager<IdentityUser> userManager)
        {
            try
            {
                var companyToDelete = await _companyRepo.GetByIdAsync(id);

                if (companyToDelete == null)
                {
                    return NotFound($"Company with ID {id} does not exist and therefore cannot be deleted.");
                }

                var email = User.FindFirstValue(ClaimTypes.Email);
                var loggedInUser = await userManager.FindByEmailAsync(email);

                // Check if the logged-in user exists
                if (loggedInUser == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        "Error retrieving the logged-in user.");
                }

                // Check if the logged-in user is trying to delete their own account or if they are an admin
                if (loggedInUser.Id != companyToDelete.IdentityUserId && !User.IsInRole("admin"))
                {
                    return Forbid();
                }

                var companyDTOToReturn = _mapper.Map<CompanyDTO>(companyToDelete);

                await _companyRepo.DeleteAsync(id);

                // Delete the associated IdentityUser
                var identityUserToDelete = await userManager.FindByIdAsync(companyToDelete.IdentityUserId);
                if (identityUserToDelete != null)
                {
                    var identityResult = await userManager.DeleteAsync(identityUserToDelete);
                    if (!identityResult.Succeeded)
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError,
                            "Error deleting associated IdentityUser from the database.");
                    }
                }

                return Ok(companyDTOToReturn);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error deleting data from the database. {ex.Message}");
            }
        }

        [HttpPut("{id:int}"), Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminCompanyPolicy")]
        public async Task<ActionResult> UpdateCompany(int id, CompanyUpdateDTO companyDTO, [FromServices] UserManager<IdentityUser> userManager)
        {
            try
            {
                // Check if CompanyDTO is provided
                if (companyDTO == null)
                {
                    return BadRequest();
                }

                // Get the existing company to update
                var existingCompany = await _companyRepo.GetByIdAsync(id);
                if (existingCompany == null)
                {
                    return NotFound($"Company with ID {id} does not exist.");
                }

                var email = User.FindFirstValue(ClaimTypes.Email);
                var loggedInUser = await userManager.FindByEmailAsync(email);

                // Check if the logged-in user exists
                if (loggedInUser == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        "Error retrieving the logged-in user.");
                }

                // Check if the logged-in user is trying to update their own account or if they have the admin role
                if (loggedInUser.Id != existingCompany.IdentityUserId && !User.IsInRole("admin"))
                {
                    return Forbid();
                }

                // Map the CompanyDTO to Company and set the ID
                var updatedCompany = _mapper.Map<Company>(companyDTO);
                updatedCompany.CompanyID = id;

                // Perform the update using the service
                await _companyRepo.UpdateAsync(updatedCompany);
                return NoContent(); // Return 204 No Content after successful update
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error updating data in the database. {ex.Message}");
            }
        }

        [HttpGet("bookings/week/{year:int}/{weekNumber:int}"), Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminCompanyPolicy")]
        public async Task<ActionResult<IEnumerable<AppointmentDTO>>> GetBookingsInWeek(int year, int weekNumber)
        {
            try
            {
                // Beräkna start- och slutdatum för den angivna veckan
                DateTime startDate = DateHelper.FirstDateOfWeek(year, weekNumber);
                DateTime endDate = startDate.AddDays(6).AddHours(23).AddMinutes(59).AddSeconds(59); // Anta 7-dagars vecka (måndag till söndag)

                
                var bookings = await _companyRepo.GetBookingsInDateRange(startDate, endDate);

                //var nonDeletedBookings = bookings.Where(b => !b.IsDeleted);
                var bookingsDTO = _mapper.Map<IEnumerable<AppointmentDTO>>(bookings);

                return Ok(bookingsDTO);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving bookings for week.");
            }
        }

        [HttpGet("bookings/month/{year:int}/{month:int}"), Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminCompanyPolicy")]
        public async Task<ActionResult<IEnumerable<AppointmentDTO>>> GetBookingsInMonth(int year, int month)
        {
            try
            {
                // Beräkna start- och slutdatum för den angivna månaden
                DateTime startDate = new DateTime(year, month, 1);
                DateTime endDate = startDate.AddMonths(1).AddDays(-1); // Sista dagen i månaden

                
                var bookings = await _companyRepo.GetBookingsInDateRange(startDate, endDate);
                
                //var nonDeletedBookings = bookings.Where(b => !b.IsDeleted);
                var bookingsDTO = _mapper.Map<IEnumerable<AppointmentDTO>>(bookings);

                return Ok(bookingsDTO);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving bookings for month.");
            }
        }
    }
}
