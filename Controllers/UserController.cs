using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MmgMapAPI.DAO;
using MmgMapAPI.Entities;
using MmgMapAPI.Service;
using MmgMapAPI.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Results;

namespace MmgMapAPI.Controllers
{
    [Route("/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private UserService _userService = new UserService(new UserDAO(), new MoDAO());

        [HttpGet]
        public ActionResult GetAll()
        {
            User currentUser = AuthManager.GetAuthUser(this.HttpContext);

            if (currentUser != null)
            {
                if (currentUser.Role == Role.ADMIN)
                {
                    List<User> result = _userService.GetAll();
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

        [HttpPost]
        public ActionResult Create([FromBody] User user)
        {
            User currentUser = AuthManager.GetAuthUser(this.HttpContext);

            if (currentUser != null)
            {
                if (currentUser.Role == Role.ADMIN)
                {
                    User result = _userService.Create(user);
                    if (result == null)
                    {
                        if (_userService.GetByUsername(user.Username) != null)
                        {
                            return new BadRequestObjectResult("Пользователь с таким именем уже существует");
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

        [HttpPut]
        public ActionResult Update([FromBody] User user)
        {
            User currentUser = AuthManager.GetAuthUser(this.HttpContext);

            if (currentUser != null)
            {
                if (currentUser.Role == Role.ADMIN)
                {
                    User result = _userService.Update(user);
                    if (result == null)
                    {
                        if (_userService.GetById(user.Id) == null)
                        {
                            return new BadRequestObjectResult("Такого пользователя нет");
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

        [HttpGet, Route("/users/enter")]
        public ActionResult Enter()
        {
            User result = AuthManager.GetAuthUser(this.HttpContext);

            if (result == null)
            {
                return StatusCode(403);
            }
            else
            {
                return new OkObjectResult(result);
            }
        }
    }
}
