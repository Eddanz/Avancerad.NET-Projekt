using AutoMapper;
using Avancerad.NET_Projekt.Services;
using Avancerad.NET_Projekt_ClassLibrary.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Avancerad.NET_Projekt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HistoryController : ControllerBase
    {
        private readonly ITrackerRepo _trackerRepo;

        public HistoryController(ITrackerRepo trackerRepo)
        {
            _trackerRepo = trackerRepo;
        }

        [HttpGet, Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminPolicy")]
        [Route("Tracker")]
        public async Task<IActionResult> GetTracker()
        {
            var trackerList = await _trackerRepo.GetAllAsync();
            return Ok(trackerList);
        }
    }
}
