# Task and Team Management System API

A comprehensive Task and Team Management System API built with .NET 8, CQRS pattern, and Vertical Slice Architecture.

## Screenshots

![Screenshot 1](contents/Screenshot%202025-12-14%20161307.png)

![Screenshot 2](contents/Screenshot%202025-12-14%20161443.png)

![Screenshot 3](contents/Screenshot%202025-12-14%20161710.png)

## Technology Stack

- **.NET 8.0** - Framework
- **ASP.NET Core** - Web API
- **Entity Framework Core** - ORM
- **SQL Server** - Database
- **ASP.NET Core Identity** - Authentication & Authorization
- **JWT** - Token-based authentication
- **MediatR** - CQRS implementation
- **Mapster** - Object mapping
- **Serilog** - Logging
- **Scalar** - API documentation
- **xUnit** - Unit testing
- **Moq** - Mocking framework
- **FluentAssertions** - Assertions

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/sql-server/sql-server-downloads) or [SQL Server LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb)
- [Docker](https://www.docker.com/get-started) (Optional, for containerized deployment)
- [Docker Compose](https://docs.docker.com/compose/install/) (Optional, for containerized deployment)

## Project Structure

```
TMS/
├── src/
│   └── TaskAndTeamManagementSystem.Api/
│       ├── Common/           # Shared utilities, DTOs, behaviors
│       ├── Domain/           # Domain entities
│       ├── Features/         # Feature-based organization (CQRS)
│       ├── Infrastructure/   # Data access, repositories, services
│       └── Program.cs        # Application entry point
├── tests/
│   └── TaskAndTeamManagementSystem.Api.Tests/
│       └── Features/         # Unit tests
└── docker-compose.yml        # Docker Compose configuration
```

## Setup Instructions

### 1. Clone the Repository

```bash
git clone <repository-url>
cd TMS
```

### 2. Database Setup

#### Option A: Using SQL Server LocalDB (Default)

The application is configured to use SQL Server LocalDB by default. No additional setup is required.

#### Option B: Using SQL Server

Update the connection string in `src/TaskAndTeamManagementSystem.Api/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=TMSDb;User Id=sa;Password=YourPassword;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

### 3. Restore Dependencies

```bash
dotnet restore
```

### 4. Build the Project

```bash
dotnet build
```

### 5. Run Database Migrations

The database will be automatically created and seeded on first run. The application uses `EnsureCreated()` in development mode.

## Running the Application

### Local Development

#### Using .NET CLI

```bash
cd src/TaskAndTeamManagementSystem.Api
dotnet run
```

The API will be available at:
- **HTTP**: `http://localhost:5173`
- **HTTPS**: `https://localhost:7116`
- **API Documentation (Scalar)**: `https://localhost:7116/scalar`

#### Using Visual Studio

1. Open `TMS.sln` in Visual Studio
2. Set `TaskAndTeamManagementSystem.Api` as the startup project
3. Press `F5` or click "Run"

### Docker Deployment

#### Using Docker Compose (Recommended)

```bash
# Build and run all services (API + SQL Server)
docker-compose up --build

# Run in detached mode
docker-compose up -d

# View logs
docker-compose logs -f api

# Stop services
docker-compose down

# Stop and remove volumes
docker-compose down -v
```

The API will be available at:
- **HTTP**: `http://localhost:8080`
- **API Documentation (Scalar)**: `http://localhost:8080/scalar`

#### Using Docker Only

```bash
# Build the image
docker build -t tms-api -f src/TaskAndTeamManagementSystem.Api/Dockerfile .

# Run the container
docker run -p 8080:8080 -e ConnectionStrings__DefaultConnection="Server=your-sql-server;Database=TMSDb;User Id=sa;Password=YourPassword;TrustServerCertificate=True" tms-api
```

## Default Users

The application comes with three pre-seeded users:

| Role     | Email              | Password    |
|----------|-------------------|-------------|
| Admin    | admin@demo.com    | Admin123    |
| Manager  | manager@demo.com  | Manager123  |
| Employee | employee@demo.com | Employee123 |

## API Documentation

Once the application is running, access the API documentation at:
- **Scalar UI**: `https://localhost:7116/scalar` (or `http://localhost:8080/scalar` in Docker)

The Scalar UI provides an interactive API documentation interface where you can:
- View all available endpoints
- Test API calls directly from the browser
- See request/response schemas
- Authenticate and use JWT tokens

## Authentication

The API uses JWT (JSON Web Token) authentication. To authenticate:

1. **Login** - POST `/api/auth/login`
   ```json
   {
     "email": "admin@demo.com",
     "password": "Admin123"
   }
   ```

2. **Use the Token** - Include the token in the Authorization header:
   ```
   Authorization: Bearer <your-token>
   ```

## API Endpoints

### Authentication
- `POST /api/auth/login` - User login

### Users
- `GET /api/users` - Get all users (Admin only)
- `GET /api/users/{id}` - Get user by ID (Admin only)
- `POST /api/users` - Create user (Admin only)
- `PUT /api/users/{id}` - Update user (Admin only)
- `DELETE /api/users/{id}` - Delete user (Admin only)

### Teams
- `GET /api/teams` - Get all teams
- `GET /api/teams/{id}` - Get team by ID
- `POST /api/teams` - Create team (Admin, Manager)
- `PUT /api/teams/{id}` - Update team (Admin, Manager)
- `DELETE /api/teams/{id}` - Delete team (Admin only)

### Tasks
- `GET /api/tasks` - Get all tasks (with pagination)
- `GET /api/tasks/{id}` - Get task by ID
- `POST /api/tasks` - Create task (Admin, Manager)
- `PUT /api/tasks/{id}` - Update task (Admin, Manager)
- `DELETE /api/tasks/{id}` - Delete task (Admin only)
- `POST /api/tasks/{id}/assign` - Assign task to user (Admin, Manager)
- `PUT /api/tasks/{id}/status` - Update task status

## Running Tests

### Run All Tests

```bash
dotnet test
```

### Run Tests for Specific Project

```bash
cd tests/TaskAndTeamManagementSystem.Api.Tests
dotnet test
```

### Run Tests with Coverage

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## Configuration

### Development Configuration

All development-specific settings are in `src/TaskAndTeamManagementSystem.Api/appsettings.Development.json`:

- **ConnectionStrings**: Database connection
- **Jwt**: JWT token settings
- **Identity**: Password and user requirements
- **Serilog**: Logging configuration
- **Swagger**: API documentation settings

### Environment Variables

You can override configuration using environment variables:

```bash
export ConnectionStrings__DefaultConnection="Server=localhost;Database=TMSDb;..."
export Jwt__Key="YourSecretKey"
export ASPNETCORE_ENVIRONMENT=Development
```

## Logging

Logs are written to:
- **Console**: Standard output
- **File**: `logs/log-YYYYMMDD.txt` (daily rolling)

## Troubleshooting

### Database Connection Issues

1. Ensure SQL Server is running
2. Check the connection string in `appsettings.Development.json`
3. Verify SQL Server authentication is enabled
4. For LocalDB, ensure it's installed and running

### Port Already in Use

If port 5173 or 7116 is already in use:

1. Change the port in `src/TaskAndTeamManagementSystem.Api/Properties/launchSettings.json`
2. Or use the `--urls` parameter:
   ```bash
   dotnet run --urls "http://localhost:5002;https://localhost:5003"
   ```

### Docker Issues

1. Ensure Docker is running
2. Check if ports 8080, 8081, or 1433 are available
3. Verify Docker Compose version: `docker-compose --version`
4. Check container logs: `docker-compose logs api`

## Development Guidelines

### Adding New Features

1. Create feature folder under `Features/`
2. Implement Command/Query handlers using MediatR
3. Add validators using FluentValidation
4. Create DTOs in the feature folder
5. Register endpoints in `Program.cs`

### Code Style

- Follow C# coding conventions
- Use meaningful names
- Keep methods focused and small
- Write unit tests for all commands and queries

## License

[Specify your license here]

## Contributing

[Add contribution guidelines here]

## Support

[Add support information here]

