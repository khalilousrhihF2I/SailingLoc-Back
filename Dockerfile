## Build
#FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
#WORKDIR /src
#COPY . .
#RUN dotnet restore
#RUN dotnet publish src/Api/Api.csproj -c Release -o /app/publish
#
## Runtime
#FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
#WORKDIR /app
#COPY --from=build /app/publish .
#ENV ASPNETCORE_URLS=http://+:8080
#EXPOSE 8080
#ENTRYPOINT ["dotnet", "Api.dll"]
# -------- Build stage --------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copie des csproj pour restore en cache
COPY src/Core/*.csproj ./Core/
COPY src/Infrastructure/*.csproj ./Infrastructure/
COPY src/Api/*.csproj ./Api/
RUN dotnet restore ./Api/Api.csproj

# Copie du reste du code
COPY src/Core ./Core
COPY src/Infrastructure ./Infrastructure
COPY src/Api ./Api

# Build + Publish (Release)
RUN dotnet publish ./Api/Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# -------- Runtime stage --------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Kestrel écoute sur 8080 dans le container
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

# Variables par défaut (surchargées par compose/env)
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "Api.dll"]
