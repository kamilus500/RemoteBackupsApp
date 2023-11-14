# Wybierz oficjalny obraz .NET 7
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Skopiuj pliki projektu do kontenera
COPY *.sln .
COPY RemoteBackupsApp.Infrastructure/*.csproj ./RemoteBackupsApp.Infrastructure/
COPY RemoteBackupsApp.Domain/*.csproj ./RemoteBackupsApp.Domain/
COPY RemoteBackupsApp.MVC/*.csproj ./RemoteBackupsApp.MVC/
COPY RemoteBackupsApp.UnitTests/*.csproj ./RemoteBackupsApp.UnitTests/

# Przywróć zależności
RUN dotnet restore

# Skopiuj cały kod źródłowy
COPY . .

# Zbuduj projekt
RUN dotnet build -c Release

# Uruchom testy (opcjonalne)
# RUN dotnet test --logger "trx;LogFileName=test_results.trx" 

# Publikuj aplikację
FROM build AS publish
RUN dotnet publish -c Release --no-build -o /app/publish

# Wybierz obraz runtime'owy
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 80

# Uruchom aplikację
ENTRYPOINT ["dotnet", "RemoteBackupsApp.MVC.dll"]