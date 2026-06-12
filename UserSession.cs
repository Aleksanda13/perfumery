using perfumery.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace perfumery
{
    public class UserSession
    {
        public int UserId { get;}

        public string LastName { get;}

        public string FirstName { get;}

        public string? Patronymic { get;}

        public string FullName => $"{LastName.Trim()} {FirstName.Trim()} {Patronymic.Trim()}";
        public string RoleName { get;}
        public bool IsAdmin => RoleName == "Администратор";
        public bool IsManager => RoleName == "Менеджер";
        public bool IsClient => RoleName == "Клиент";
        public bool IsGuest {  get;}

        public UserSession()
        {
            UserId = 0;
            LastName = "";
            FirstName = "";
            Patronymic = "";
            RoleName = "Гость";
            IsGuest = true;

        }

        public UserSession(User user)
        {
            UserId = user.UserId;
            LastName = user.LastName;
            FirstName = user.FirstName;
            Patronymic = user.Patronymic;
            RoleName = user.Role.RoleName;
            IsGuest= false;
        }

        public static UserSession Guest() => new UserSession();
    }
}
