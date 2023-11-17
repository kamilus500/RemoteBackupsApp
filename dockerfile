# Wybierz oficjalny obraz .NET 7
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

COPY *.sln .
COPY RemoteBackupsApp.Infrastructure/*.csproj ./RemoteBackupsApp.Infrastructure/
COPY RemoteBackupsApp.Domain/*.csproj ./RemoteBackupsApp.Domain/
COPY RemoteBackupsApp.MVC/*.csproj ./RemoteBackupsApp.MVC/
COPY RemoteBackupsApp.UnitTests/*.csproj ./RemoteBackupsApp.UnitTests/

RUN dotnet restore

COPY . .

RUN dotnet build -c Release

FROM build AS publish
RUN dotnet publish -c Release --no-build -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 80

ENTRYPOINT ["dotnet", "RemoteBackupsApp.MVC.dll"]