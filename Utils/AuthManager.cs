using Microsoft.AspNetCore.Http;
using MmgMapAPI.DAO;
using MmgMapAPI.Entities;
using MmgMapAPI.Service;
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MmgMapAPI.Utils
{
    public class AuthManager
    {
        private static UserService _userService = new UserService(new UserDAO(), new MoDAO());

        //Возвращает true если пользователь с переданными username и password есть в БД и в ActiveDirectory
        public static bool IsAuth(HttpContext context)
        {
            bool result = false;

            if (IsLocatedInDB(context) && IsLocatedInAD(context))
            {
                result = true;
            }

            return result;
        }

        //Возвращает объект User если пользователь с переданными username и password есть в БД и в ActiveDirectory или null если нет
        public static User GetAuthUser(HttpContext context)
        {
            User result = null;

            if (IsLocatedInDB(context) && IsLocatedInAD(context))
            {
                string[] credentials = ParseCredentials(context);
                result = _userService.GetByUsername(credentials[0]);
            }

            return result;
        }

        //Проверяет наличие пользователя в БД
        private static bool IsLocatedInDB(HttpContext context)
        {
            bool result = false;

            string[] credentials = ParseCredentials(context);

            User user = _userService.GetByUsername(credentials[0]);

            if (user != null && user.isActive)
            {
                result = true;
            }

            return result;
        }

        //Проверяет наличие пользователя в ActiveDirectory
        private static bool IsLocatedInAD(HttpContext context)
        {
            bool result = false;

            string[] credentials = ParseCredentials(context);

            try
            {
                using (PrincipalContext pc = new PrincipalContext(ContextType.Domain))
                {
                    result = pc.ValidateCredentials(credentials[0], credentials[1]);
                }
            }
            catch (Exception)
            {
                return false;
            }

            return result;
        }

        //Раскодирует и парсит входящую строку. Возвращает массив с username и password
        private static string[] ParseCredentials(HttpContext context)
        {
            string authHeader = context.Request.Headers["Authorization"];
            string encodedUsernamePassword = authHeader.Substring("Basic ".Length).Trim();

            string[] result = new string[2];

            Encoding encoding = Encoding.GetEncoding("iso-8859-1");
            string usernamePassword = encoding.GetString(Convert.FromBase64String(encodedUsernamePassword));

            string[] credentials = usernamePassword.Split(":");
            string username = credentials[0];
            string password = "";

            for (int i = 1; i < credentials.Length; i++)
            {
                password += credentials[i];
                if (i != credentials.Length - 1)
                {
                    password += ":";
                }
            }

            result[0] = username;
            result[1] = password;

            return result;
        }
    }
}
