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
    [Route("/weeks")]
    [ApiController]
    public class WeekController : ControllerBase
    {
        private WeekService _weekService = new WeekService(new WeekDAO());

        [HttpGet]
        public ActionResult GetAll([FromQuery] int year)
        {
            List<Week> result = _weekService.GetByYear(year);
            return new OkObjectResult(result);
        }

        [HttpGet, Route("/years")]
        public ActionResult GetAllYears()
        {
            List<int> result = _weekService.GetYears();
            return new OkObjectResult(result);
        }

        [HttpPost]
        public ActionResult Create([FromBody] Week week)
        {
            User currentUser = AuthManager.GetAuthUser(this.HttpContext);

            if (currentUser != null)
            {
                if (currentUser.Role == Role.ADMIN)
                {
                    Week result = _weekService.Create(week);
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
        public ActionResult Update([FromBody] Week week)
        {
            User currentUser = AuthManager.GetAuthUser(this.HttpContext);

            if (currentUser != null)
            {
                if (currentUser.Role == Role.ADMIN)
                {
                    Week result = _weekService.Update(week);
                    if (result == null)
                    {
                        if (_weekService.GetById(week.Id) == null)
                        {
                            return new BadRequestObjectResult("Такой недели в базе данных нет");
                        }
                        else
                        {
                            return new BadRequestObjectResult("Неизвестная ошибка");
                        }
                    }
                    else
                    {
                        return new OkObjectResult(result);
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

        [HttpDelete]
        public ActionResult Delete([FromQuery] int weekId)
        {
            User currentUser = AuthManager.GetAuthUser(this.HttpContext);

            if (currentUser != null)
            {
                if (currentUser.Role == Role.ADMIN)
                {
                    bool result = _weekService.Delete(weekId);

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
