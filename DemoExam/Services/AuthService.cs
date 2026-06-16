using DemoExam.Data;
using DemoExam.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace DemoExam.Services
{
    public class AuthService
    {
        // Вспомогательный класс аутентификации пользователей в системе
        public static User? Authenticate(string login, string password)
        {
            using var db = new AppDbContext();
            var user = db.Users.Include(u => u.Role)
                .FirstOrDefault(u => u.Login == login && u.PasswordHash == password);
            return user;
        }

        public static User? GetGuestUser()
        {
            using var db = new AppDbContext();
            return db.Users.Include(u => u.Role)
                .FirstOrDefault(u => u.Role == null || u.Role.RoleName == "Guest");
        }
    }
}
