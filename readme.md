# 🎥 MyFilm - Plateforme de Gestion et Recommandation de Films

MyFilm  est une application web développée en **ASP.NET Core Blazor**, qui permet aux utilisateurs d'explorer, rechercher, organiser et ajouter leurs films préférés à une liste de favoris. L'application intègre également un **système de recommandation basé sur le profil utilisateur** et l'historique d'interaction.


## 🚀 **Fonctionnalités Principales**

### 🎬 **Exploration et Gestion des Films**
- Affichage dynamique des films disponibles (avec affichage de l'affiche, du titre, du genre, de l'année et du score IMDb).
- Recherche avancée via filtres (genre, année, score IMDb).
- Consultation des détails d’un film avec un lien vers IMDb.

### ⭐ **Gestion des Favoris**
- Ajout et suppression de films à la liste de favoris d'un utilisateur.
- Affichage des films favoris dans une section dédiée.

### 🎯 **Système de Recommandation**
L'algorithme de **recommandation personnalisé** utilise :
- **L'historique d’interaction de l’utilisateur** : Les films ajoutés en favoris et leur genre influencent les recommandations.
- **Le filtrage collaboratif** : Comparaison avec des utilisateurs ayant des préférences similaires.
- **Le score de similarité** : Calcul basé sur les genres préférés et les films regardés.

### 🔐 **Authentification et Gestion des Utilisateurs**
- Inscription et connexion sécurisée via **JWT Token**.
- Différents rôles utilisateur (**Admin, User**).
- Gestion des utilisateurs pour l’administrateur.

### 📦 **Seed de la Base de Données**
- Lors du premier démarrage, des **films et utilisateurs fictifs** sont ajoutés pour permettre de tester immédiatement l’application.
- Génération des données à partir de l’**API OMDb**.


## 📌 **Installation et Déploiement**

### 1️⃣ **Cloner le Projet**
```sh
git clone 
cd filmit https://gitlab.com/service_web_asp.net/filmit.git


### 2️⃣ **Installer les dépendances**
Assurez-vous d'avoir **.NET 6 ou supérieur** installé. Puis, exécutez :
```sh
dotnet restore
```

### 3️⃣ **Configurer la Base de Données**
L’application utilise **Entity Framework Core** et **SQLite**. Pour appliquer les migrations et générer la base de données :
```sh
dotnet ef database update
```

### 4️⃣ **Lancer l’Application**
```sh
dotnet run

L’application sera accessible sur **http://localhost:5000/**.


## 📚 **Technologies et Bibliothèques Utilisées**

| Composant | Technologie |
|-----------|------------|
| Frontend  | Blazor WebAssembly |
| Backend   | ASP.NET Core Web API |
| Base de Données | SQLite avec Entity Framework Core |
| Authentification | JWT Token |
| Recommandations | Algorithmes de filtrage collaboratif |
| API Externe | OMDb API pour récupérer les informations de films |
| Sécurité | ASP.NET Identity, Middleware Auth |

---

## 🔧 **Liste des Packages Nécessaires**
Avant de compiler, installez ces bibliothèques :

dotnet add package Microsoft.AspNetCore.Identity
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package CsvHelper
dotnet add package System.Text.Json
dotnet add package System.Net.Http.Json
dotnet add package Newtonsoft.Json

# Packages pour Machine Learning avec ML.NET
dotnet add package Microsoft.ML
dotnet add package Microsoft.ML.Data
dotnet add package Microsoft.ML.Recommender

# Packages pour manipuler les CSV et gérer les données
dotnet add package CsvHelper
dotnet add package Microsoft.Data.Analysis
dotnet add package System.Data.SQLite
dotnet add package MathNet.Numerics


---

## 🛠 **Architecture du Projet**

filmit/
│── Backend/
│   ├── Controllers/          # API Controllers (Films, Utilisateurs, Favoris)
│   ├── Models/               # Modèles de données (Film, User, Favorite, Recommendation)
│   ├── Services/             # Services de gestion (Films, Favoris, Utilisateurs, Recommandations)
│   ├── Data/                 # Configuration Entity Framework
│   ├── SeedData.cs           # Fichier pour ajouter des données de test au lancement
│   ├── appsettings.json      # Configuration de la BDD et API OMDb
│── Frontend/
│   ├── Components/           # Composants Blazor
│   ├── Pages/                # Pages principales (Films, Favoris, Suggestions)
│   ├── Services/             # Services HTTP pour communiquer avec l'API
│   ├── wwwroot/              # Fichiers CSS/Images
│── README.md                 # Documentation (ce fichier)
```

---

## 🎯 **Fonctionnement du Système de Recommandation**

L'algorithme analyse :
1. **Les genres favoris de l'utilisateur** en fonction des films qu'il a ajoutés à ses favoris.
2. **La similarité avec d'autres utilisateurs** qui ont des goûts similaires.
3. **Un score de pertinence** est calculé pour recommander des films adaptés.


## 🚀 **Améliorations Futures**
✅ **Filtrage par acteurs et réalisateurs**  
✅ **Ajout d'un système de notation des films**  
✅ **Recommandations basées sur la popularité globale**  
✅ **Déploiement sur Azure ou AWS**  

---

## 🎉 **Contribuer au Projet**
👨‍💻 **Vous souhaitez améliorer MyFilm ?
Clonez le repo, ajoutez vos fonctionnalités et faites une PR !


git checkout -b feature/nom-fonctionnalite
git commit -m "Ajout de la fonctionnalité X"
git push origin feature/nom-fonctionnalite



