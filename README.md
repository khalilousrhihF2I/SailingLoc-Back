# SailingLoc — API Backend

API REST .NET 8 pour la plateforme de location de bateaux SailingLoc.

## Démarrage Rapide

```bash
# Restaurer les dépendances
dotnet restore

# Lancer en développement
cd src/Api
dotnet run

# Ou avec Docker
docker compose up --build -d
```

**Swagger UI** : `http://localhost:5000/swagger`

## Comptes de Test

| Rôle | Email | Mot de passe |
|------|-------|-------------|
| Admin | `admin@local.test` | `Admin123` |
| Locataire | `Renter@local.test` | `Renter123` |
| Propriétaire | `Owner@local.test` | `Owner123` |

## Documentation Complète

La documentation technique complète est disponible dans le **[Wiki SailingLoc-Back](../SailingLoc-Back.wiki/Home.md)** :

- [Architecture](../SailingLoc-Back.wiki/Backend/Architecture.md)
- [Référence API](../SailingLoc-Back.wiki/Backend/API.md)
- [Modèle de Données](../SailingLoc-Back.wiki/Backend/Data-Model.md)
- [Services](../SailingLoc-Back.wiki/Backend/Services.md)
- [Sécurité](../SailingLoc-Back.wiki/Backend/Security.md)
- [Déploiement](../SailingLoc-Back.wiki/Backend/Deployment.md)
- [Tests](../SailingLoc-Back.wiki/Backend/Tests.md)
 
