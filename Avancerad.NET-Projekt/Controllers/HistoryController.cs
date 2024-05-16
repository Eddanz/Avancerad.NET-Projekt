using AutoMapper;
using Avancerad.NET_Projekt.Services;
using Avancerad.NET_Projekt_ClassLibrary.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Avancerad.NET_Projekt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HistoryController : ControllerBase
    {
        private readonly IHistoryRepo _historyRepo;
        private readonly IMapper _mapper;

        public HistoryController(IHistoryRepo historyRepo, IMapper mapper)
        {
            _historyRepo = historyRepo;
            _mapper = mapper;
        }

        [HttpGet("history")]
        public async Task<ActionResult<IEnumerable<HistoryDTO>>> GetAllHistory()
        {
            try
            {
                var historyList = await _historyRepo.GetAllAsync();

                // Map historyList to HistoryDTOs
                var historyDTOs = _mapper.Map<IEnumerable<HistoryDTO>>(historyList);

                return Ok(historyDTOs);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error retrieving data from the database.");
            }
        }

        [HttpGet("history/{id:int}")]
        public async Task<ActionResult<IEnumerable<HistoryDTO>>> GetHistory(int id)
        {
            try
            {
                var historyList = await _historyRepo.GetByIdAsync(id);

                if (historyList == null || !historyList.Any())
                {
                    return NotFound($"No history found for appointment with ID {id}.");
                }

                // Map historyList to HistoryDTOs
                var historyDTOs = _mapper.Map<IEnumerable<HistoryDTO>>(historyList);

                return Ok(historyDTOs);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error retrieving data from the database.");
            }
        }
    }
}
