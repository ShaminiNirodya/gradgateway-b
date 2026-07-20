# GradGateway Backend API

This repository contains the ASP.NET Core backend for GradGateway, a platform that connects undergraduates, companies, and recruiters for internships, graduate roles, project showcases, applications, and messaging.

## What this backend provides

- User authentication and authorization with Firebase JWT tokens
- Student, company, opportunity, application, project, and team management
- Real-time communication support through the API layer
- Swagger/OpenAPI documentation for easy integration
- SQL Server persistence via Entity Framework Core

## Tech stack

- .NET 9
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- Swagger / OpenAPI
- Firebase authentication

## Project structure

- GradGateway.Api - API host, controllers, Swagger, authentication, and startup configuration
- GradGateway.Business - business logic, services, DTOs, and interfaces
- GradGateway.Data - EF Core context, entities, and data access layer

## Getting started

1. Open the solution:
   ```bash
   dotnet restore GradGateway.sln
   ```
2. Configure your database and Firebase settings in the API configuration files.
3. Run the API:
   ```bash
   dotnet run --project GradGateway.Api
   ```
4. Open the Swagger UI in your browser (development environment):
   ```text
   https://localhost:7059/
   ```

## Useful commands

```bash
dotnet build GradGateway.sln
dotnet run --project GradGateway.Api
```

## Notes

- The API is designed to work with the GradGateway frontend running on the Next.js client.
- CORS is configured for local frontend development on port 3000.
