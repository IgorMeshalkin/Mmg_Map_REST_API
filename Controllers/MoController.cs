using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MmgMapAPI.DAO;
using MmgMapAPI.DTO;
using MmgMapAPI.Entities;
using MmgMapAPI.Service;
using MmgMapAPI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MmgMapAPI.Controllers
{
    [Route("/mos")]
    [ApiController]
    public class MoController : ControllerBase
    {
        private MoService _moService = new MoService(new MoDAO());
        private DataService _dataService = new DataService(new DataDAO(), new MoDAO());
        private WeekService _weekService = new WeekService(new WeekDAO());

        [HttpGet]
        public ActionResult GetAll()
        {
            List<MoDTO> result = new List<MoDTO>();

            _moService.GetAll().ForEach(mo =>
            {
                result.Add(new MoDTO(mo, _dataService.GetPartialDataByMoAndWeek(mo.Id, _weekService.GetLastWeek().Id)));
            });

            return new OkObjectResult(result);
        }

        [HttpGet, Route("/mos/by_user")]
        public ActionResult GetByUserId([FromQuery] int userId)
        {
            List<MoDTO> result = new List<MoDTO>();

            _moService.GetByUserId(userId).ForEach(mo =>
            {
                result.Add(new MoDTO(mo, _dataService.GetPartialDataByMoAndWeek(mo.Id, _weekService.GetLastWeek().Id)));
            });

            return new OkObjectResult(result);
        }

        [HttpPost]
        public ActionResult Create([FromBody] Mo mo)
        {
            User currentUser = AuthManager.GetAuthUser(this.HttpContext);

            if (currentUser != null)
            {
                if (currentUser.Role == Role.ADMIN)
                {
                    Mo result = _moService.Create(mo);
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
        public ActionResult Update([FromBody] Mo mo)
        {
            User currentUser = AuthManager.GetAuthUser(this.HttpContext);

            if (currentUser != null)
            {
                Mo result = _moService.Update(mo);
                if (result == null)
                {
                    if (_moService.GetById(mo.Id) == null)
                    {
                        return new BadRequestObjectResult("Такой медицинской организации нет");
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
                return StatusCode(403);
            }
        }
    }
}
