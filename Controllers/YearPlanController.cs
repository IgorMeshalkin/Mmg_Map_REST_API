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
    [Route("/year_plans")]
    [ApiController]
    public class YearPlanController : ControllerBase
    {
        private YearPlanService _yearPlanService = new YearPlanService(new YearPlanDAO());

        [HttpGet]
        public ActionResult GetAll([FromQuery] int year, [FromQuery] int mosId)
        {
            List<YearPlan> result = _yearPlanService.GetByYearAndMo(year, mosId);
            return new OkObjectResult(result);
        }

        [HttpPost]
        public ActionResult Create([FromBody] YearPlan yearPlan, [FromQuery] int mosId)
        {
            User currentUser = AuthManager.GetAuthUser(this.HttpContext);

            if (currentUser != null)
            {
                if (currentUser.Role == Role.ADMIN)
                {
                    YearPlan result = _yearPlanService.Create(yearPlan, mosId);
                    return new OkObjectResult(result);
                }
                else
                {
                    return StatusCode(401);
                }
            }
            else
            {
                return StatusCode(403);
            }
        }

        [HttpPut]
        public ActionResult Update([FromBody] YearPlan yearPlan, [FromQuery] int mosId)
        {
            User currentUser = AuthManager.GetAuthUser(this.HttpContext);

            if (currentUser != null)
            {
                if (currentUser.Role == Role.ADMIN)
                {
                    YearPlan result = _yearPlanService.Update(yearPlan, mosId);
                    return new OkObjectResult(result);
                }
                else
                {
                    return StatusCode(401);
                }
            }
            else
            {
                return StatusCode(403);
            }
        }

        [HttpDelete]
        public ActionResult Delete([FromQuery] int yearPlanId)
        {
            User currentUser = AuthManager.GetAuthUser(this.HttpContext);

            if (currentUser != null)
            {
                if (currentUser.Role == Role.ADMIN)
                {
                    bool result = _yearPlanService.Delete(yearPlanId);

                    if (result)
                    {
                        return new OkObjectResult(result);
                    }
                    else
                    {
                        return new BadRequestObjectResult(result);
                    }
                }
                else
                {
                    return StatusCode(401);
                }
            }
            else
            {
                return StatusCode(403);
            }
        }
    }
}
