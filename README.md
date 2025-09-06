# RemoteBackupsApp

A web application for secure cloud file storage and management, built with ASP.NET Core MVC.

## Features

- User registration and login
- Password change
- File upload with size limit (up to 100 MB)
- Browse, download, and delete own files
- File upload progress panel (SignalR)
- Toast notifications (NToastNotify)
- Background file upload queue processing (BackgroundService)
- Responsive user interface (Bootstrap 5)

## Requirements

- .NET 9.0
- Docker (optional, for running the database)

## Installation

1. **Clone the repository:**
   ```sh
   git clone https://github.com/your-login/RemoteBackupsApp.git
   cd RemoteBackupsApp
   ```

2. **Start the database (optionally via Docker):**
   ```sh
   docker-compose up -d
   ```

3. **Initialize the database:**
   - Use the `database_initializer.sql` file to create the required tables.

4. **Build and run the application:**
   ```sh
   dotnet build
   dotnet run --project RemoteBackupsApp.MVC
   ```

5. **Open in your browser:**
   ```
   https://localhost:5001
   ```

## Project Structure

- `RemoteBackupsApp.Domain` – domain models and interfaces
- `RemoteBackupsApp.Infrastructure` – business logic, services, BackgroundService
- `RemoteBackupsApp.MVC` – presentation layer (MVC), views, controllers, static assets

## Licenses

The project uses open source libraries (Bootstrap, jQuery, SignalR, NToastNotify) – see details in the `wwwroot/lib` directory.

## Author

Project created by kamilus500.

---
