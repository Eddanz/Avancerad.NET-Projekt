using AutoMapper;
using Avancerad.NET_Projekt.Methods;
using Avancerad.NET_Projekt.Services;
using Avancerad.NET_Projekt_ClassLibrary.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Avancerad.NET_Projekt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyRepo _companyRepo;
        private readonly IMapper _mapper;

        public CompanyController(ICompanyRepo companyRepo, IMapper mapper)
        {
            _companyRepo = companyRepo;
            _mapper = mapper;
        }

        [HttpGet]
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

        [HttpGet("{id:int}")]
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

        [HttpPost]
        public async Task<ActionResult<CompanyDTO>> CreateCompany(CompanyDTO newCompanyDTO)
        {
            try
            {
                if (newCompanyDTO == null)
                {
                    return BadRequest();
                }

                var newCompany = _mapper.Map<Company>(newCompanyDTO);

                var createdCompany = await _companyRepo.CreateAsync(newCompany);

                var createdCompanyDTO = _mapper.Map<CompanyDTO>(createdCompany);

                return CreatedAtAction(nameof(GetCompany),
                    new { id = createdCompany.CompanyID },
                    createdCompanyDTO);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error creating data in the database.");
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<CompanyDTO>> DeleteCompany(int id)
        {
            try
            {
                var companyToDelete = await _companyRepo.GetByIdAsync(id);

                if (companyToDelete == null)
                {
                    return NotFound($"Company with ID {id} does not exist and therefore cannot be deleted.");
                }

                var companyDTOToReturn = _mapper.Map<CompanyDTO>(companyToDelete);

                await _companyRepo.DeleteAsync(id);

                return Ok(companyDTOToReturn);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error deleting data from the database.");
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> UpdateCompany(int id, CompanyDTO companyDTO)
        {
            try
            {
                if (companyDTO == null)
                {
                    return BadRequest();
                }

                var existingCompany = await _companyRepo.GetByIdAsync(id);
                if (existingCompany == null)
                {
                    return NotFound($"Company with ID {id} does not exist.");
                }

                var updatedCompany = _mapper.Map<Company>(companyDTO);
                updatedCompany.CompanyID = id;

                await _companyRepo.UpdateAsync(updatedCompany);
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error updating data in the database.");
            }
        }

        [HttpGet("bookings/week/{year:int}/{weekNumber:int}")]
        public async Task<ActionResult<IEnumerable<AppointmentDTO>>> GetBookingsInWeek(int year, int weekNumber)
        {
            try
            {
                // Beräkna start- och slutdatum för den angivna veckan
                DateTime startDate = DateHelper.FirstDateOfWeek(year, weekNumber);
                DateTime endDate = startDate.AddDays(6); // Anta 7-dagars vecka (måndag till söndag)

                // Hämta alla bokningar för den angivna veckan som inte är flaggade som raderade
                var bookings = await _companyRepo.GetBookingsInDateRange(startDate, endDate);
                var nonDeletedBookings = bookings.Where(b => !b.IsDeleted);

                return Ok(nonDeletedBookings);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving bookings for week.");
            }
        }

        [HttpGet("bookings/month/{year:int}/{month:int}")]
        public async Task<ActionResult<IEnumerable<AppointmentDTO>>> GetBookingsInMonth(int year, int month)
        {
            try
            {
                // Beräkna start- och slutdatum för den angivna månaden
                DateTime startDate = new DateTime(year, month, 1);
                DateTime endDate = startDate.AddMonths(1).AddDays(-1); // Sista dagen i månaden

                // Hämta alla bokningar för den angivna månaden som inte är flaggade som raderade
                var bookings = await _companyRepo.GetBookingsInDateRange(startDate, endDate);
                var nonDeletedBookings = bookings.Where(b => !b.IsDeleted);

                return Ok(nonDeletedBookings);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving bookings for month.");
            }
        }
    }
}
