
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Reflection;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; // For AllowAnonymous
using Microsoft.Extensions.Logging;

using IBCQC_NetCore.Models;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace IBCQC_NetCore.Controllers
{

    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]/{format:int=0}")]  // {} = optional
    [Route("[controller]/{format:int=0}")]  // {} = optional
    public class GetInfoController : ControllerBase
    {
        private readonly ILogger<GetInfoController> _logger;

        public GetInfoController(ILogger<GetInfoController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Get the version number and related information of this API
        /// </summary>
        /// <param name="model"></param>
        /// <param name="format">Format of version number 0=decimal (default), 1=hex</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Get([FromQuery] GetInfoViewModel model)
        {
            int whichFormat = model.Format;
            var myVersion = Assembly.GetExecutingAssembly().GetName().Version;
            string myVariant = " (CSMP:NoKEM)";
            // "LT"  = C# Lite - sources reduced to API only
            // "IDB" = C# Lite for IDB
            // "CSMP" = C-Sharp Multi-Platform (.NET Core)

            GetInfoResponse response = new GetInfoResponse();
            response.major = myVersion.Major;
            response.minor = myVersion.Minor;
            response.build = myVersion.Build;
            response.revision = myVersion.Revision;
            response.variant = myVariant;
            response.buildDate = System.IO.File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location);
            response.configuration = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyConfigurationAttribute>().Configuration;
            response.targetFramework = Assembly.GetExecutingAssembly().GetCustomAttribute<System.Runtime.Versioning.TargetFrameworkAttribute>().FrameworkName;
            response.supportedEndpoints = "getinfo" + "|" + // Search solution for "[Route("
                                          "api/getinfo" + "|" +
                                          "api/login" + "|" +
                                          "api/reqkeypair" + "|" +
                                          "api/setupclient" + "|" +
                                          "api/sharedsecret" + "|" +
                                          "api/srng" + "|" +
                                          "api/verifykeys";

            switch (whichFormat)
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
            _logger.LogInformation($"[{DateTime.UtcNow.ToLongTimeString()}] GetInfo called");

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
        public DateTime buildDate { get; set; }
        public string configuration { get; set; }
        public string targetFramework { get; set; }
        public string supportedEndpoints { get; set; }
    }
}
