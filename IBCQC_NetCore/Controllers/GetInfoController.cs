using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using IBCQC_NetCore.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace IBCQC_NetCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GetInfoController : ControllerBase
    {
      
        /// <summary>
        /// Get the version number of this API
        /// </summary>
        /// <param name="model"></param>
        /// <param name="format">Format of version number 0=decimal (default), 1=hex</param>
        /// <returns></returns>
        //[Route("{format}")]    // {} = optional
        //[Route("{format:int=0}")]    // {} = optional
        [Route("getinfo/{format:int=0}")]    // {} = optional
        [HttpGet]
        public IActionResult Get(GetInfoViewModel model)
        {
            
            var myVersion = Assembly.GetExecutingAssembly().GetName().Version;
            string myVariant = " (IDB)"; 
                                         // "LT"  = C# Lite - sources reduced to API only
                                         // "IDB" = C# Lite for IDB
                                        

            GetInfoResponse response = new GetInfoResponse();
            response.major = myVersion.Major;
            response.minor = myVersion.Minor;
            response.build = myVersion.Build;
            response.revision = myVersion.Revision;
            response.variant = myVariant;

            switch (model.Format)
            {
                case 0: // Decimal
                default:
                    // It seems that myVersion.ToString() returns Minor and Build swapped around.
                    // So we'll do it manually...
                    // 0001.02.03.04 (IDB)
                    response.versionStr = string.Format("{0:d04}.{1:d02}.{2:d02}.{3:d02}{4}",
                                               myVersion.Major,
                                               myVersion.Minor,
                                               myVersion.Build,
                                               myVersion.Revision,
                                               myVariant);
                    break;
                case 1: // Hex
                    response.versionStr = string.Format("{0:X04}.{1:X02}.{2:X02}.{3:X02}{4}",
                                               myVersion.Major,
                                               myVersion.Minor,
                                               myVersion.Build,
                                               myVersion.Revision,
                                               myVariant);
                    // For info, the .MajorRevision and .MinorRevision fields return
                    // the Most and Least significant 16 bits of .Revision, respectively.
                    break;
            }
         
            return Ok(response);
           
        }

    }
    public class GetInfoResponse
    {
        public int major { get; set; }
        public int minor { get; set; }
        public int build { get; set; }
        public int revision { get; set; }
        public string variant { get; set; }
        public string versionStr { get; set; }
    }
}
