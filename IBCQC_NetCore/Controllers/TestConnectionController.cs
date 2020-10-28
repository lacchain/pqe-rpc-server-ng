using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;



namespace IBCQC_NetCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestConnectionController : ControllerBase
    {
        // GET: api/<TestConnectionController>
        [HttpGet]
        public IActionResult Get()
        {
            return StatusCode(200, "Alive");
        }

       
    }
}
