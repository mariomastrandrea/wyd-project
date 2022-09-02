using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _13_Wyd.ModelClasses;
using _13_Wyd.RestApi.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace _13_Wyd.RestApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogController : ControllerBase
    {
        private readonly ILogRepository LogRepository;


        public LogController(ILogRepository logRepository)
        {
            this.LogRepository = logRepository;
        }

        // GET api/log?top={num}
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Log>>> GetTopLogs(int? top)
        {
            if (top == null || top <= 0)
                return BadRequest();

            try
            {
                IEnumerable<Log> topLogs = await this.LogRepository.GetTopLogs((int)top);

                if (topLogs == null)
                    return NotFound();

                return Ok(topLogs);
            }
            catch(Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                            "Error retrieving data from the database");
            }
        }

        // POST api/log
        [HttpPost]
        public async Task<ActionResult<Log>> CreateLog([FromBody] Log newLog)
        {
            try
            {
                if (newLog == null || string.IsNullOrWhiteSpace(newLog.Id))
                    return BadRequest();

                Log createdLog = await this.LogRepository.CreateLog(newLog);

                return StatusCode(201, createdLog);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                            "Error saving data in the database");
            }
        }
    }
}
