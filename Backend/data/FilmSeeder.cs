using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using WebApi.Models;

namespace WebApi.Services // Ensure this namespace matches your project
{
    public class FilmSeeder
    {
public static void SeedFilms(BddContext context, string csvFilePath)
{
    // 🛑 Suppression des films existants pour éviter le message "Films already exist"
    context.Films.ExecuteDelete();
    context.SaveChanges();

    // 🔍 Vérifie si le fichier CSV existe
    if (!File.Exists(csvFilePath))
    {
        Console.WriteLine($"⚠️ Fichier CSV introuvable : {csvFilePath}");
        return;
    }

    try
    {
        using var reader = new StreamReader(csvFilePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        var films = csv.GetRecords<Film>().ToList();

        if (films == null || !films.Any())
        {
            Console.WriteLine("⚠️ Aucun film trouvé dans le fichier CSV.");
            return;
        }

        // ✅ Ajout des films à la base de données
        context.Films.AddRange(films);
        context.SaveChanges();
        Console.WriteLine($"✅ {films.Count} films ajoutés avec succès !");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Erreur lors du chargement des films : {ex.Message}");
    }
}

    }
}
