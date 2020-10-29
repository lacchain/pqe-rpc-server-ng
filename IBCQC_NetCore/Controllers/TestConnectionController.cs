using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IBCQC_NetCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestConnectionController : ControllerBase
    {
        private readonly ILogger<TestConnectionController> _logger;

        public TestConnectionController(ILogger<TestConnectionController> logger)
        {
            _logger = logger;
        }


        // GET: api/<TestConnectionController>
        [HttpGet]
        public IActionResult Get()
        {
            _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] TestConnection  called");

            _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] Returning Success from TestConnection ");
            return StatusCode(200, "Alive");
        }

       
    }
}
