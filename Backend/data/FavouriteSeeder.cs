using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using WebApi.Models;

namespace WebApi.Data
{
    public class FavouriteSeeder
    {
       public static void SeedFavourites(BddContext context)
{
    var random = new Random();

    var films = context.Films.ToList();
    var users = context.Users.ToList();

    if (!films.Any() || !users.Any())
    {
        Console.WriteLine("No films or users found to associate.");
        return;
    }

    if (context.Favourites.Any())
    {
        Console.WriteLine("Favourites already exist in the database.");
        return;
    }

    var favourites = new List<Favourite>();

    foreach (var user in users)
    {
        var filmCount = random.Next(5, 8);
        var selectedFilms = films.OrderBy(_ => random.Next()).Take(filmCount);

        foreach (var film in selectedFilms)
        {
            favourites.Add(new Favourite
            {
                UserId = user.Id,
                FilmId = film.Id,
                Rating = random.Next(1, 11)
            });
        }
    }

    context.Favourites.AddRange(favourites);
    context.SaveChanges();
    Console.WriteLine($"Reseeded {favourites.Count} favourite film associations successfully!");
}

    }
}
