using System;
using System.Collections.Generic;
using System.Linq;
using WebApi.Models; // Ensure this namespace matches your project
using Microsoft.EntityFrameworkCore;

namespace WebApi.Data // Match your project namespace
{
    public class UserSeeder
    {
        
        public static void SeedUsers(BddContext context)
        {
            context.Users.ExecuteDelete();
            context.SaveChanges();
            if (context.Users.Any())
            {
                Console.WriteLine("Users already exist in the database.");
                return;
            }

        var users = new List<User>();

        for (int i = 1; i <= 20; i++)
        {
            users.Add(new User
            {
                Pseudo = $"User{i}",
                Password = "defaultPassword",
                Role = (Role)(i % 2)
            });
        }

    context.Users.AddRange(users);
    context.SaveChanges();
    Console.WriteLine("20 users have been seeded successfully.");
}

    }
}
