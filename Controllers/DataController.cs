using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MmgMapAPI.DAO;
using MmgMapAPI.Entities;
using MmgMapAPI.Service;
using MmgMapAPI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MmgMapAPI.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    public class DataController : ControllerBase
    {
        private DataService _dataService = new DataService(new DataDAO(), new MoDAO());

        [HttpGet, Route("/full_data")]
        public ActionResult GetAll([FromQuery] int mosId, [FromQuery] int weekId)
        {
            List<FullData> result = _dataService.GetFullDataByMoAndWeek(mosId, weekId);
            return new OkObjectResult(result);
        }

        [HttpPost, Route("/data")]
        public ActionResult Save([FromBody] List<FullData> data, [FromQuery] int mosId, [FromQuery] int weekId)
        {
            User currentUser = AuthManager.GetAuthUser(this.HttpContext);

            if (currentUser != null)
            {
                List<FullData> result = new List<FullData>();

                data.ForEach(d =>
                {
                    if (d.Id != null)
                    {
                        FullData updatedData = _dataService.Update(d);
                        result.Add(updatedData);
                    }
                    else
                    {
                        FullData createdData = _dataService.Create(d, mosId, weekId);
                        result.Add(createdData);
                    }
                });
                return new OkObjectResult(result);
            }
            else
            {
                return StatusCode(403);
            }
        }
    }
}
