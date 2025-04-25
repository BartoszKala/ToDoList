# Etap 'base' - ASP.NET runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Etap 'build' - Budowanie aplikacji z .NET SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ToDoList/ToDoList.csproj", "ToDoList/"]
RUN dotnet restore "./ToDoList/ToDoList.csproj"
COPY . . 
WORKDIR "/src/ToDoList"
RUN dotnet build "./ToDoList.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Etap 'publish' - Publikacja aplikacji
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ToDoList.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Etap 'final' - Aplikacja gotowa do uruchomienia z .NET SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Uruchomienie aplikacji
ENTRYPOINT ["dotnet", "ToDoList.dll"]
