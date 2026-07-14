# Class Assignment System API

A backend Web API for managing classes, teachers, students, and enrollments — built with **Clean Architecture**, **CQRS**, and modern **.NET** practices. This project was built as a portfolio piece to demonstrate production-grade backend engineering: layered architecture, JWT-based authentication, role-based authorization, and secure password handling.

## Features

- **Role-Based Access Control (RBAC)** — three roles: `Admin`, `Teacher`, `Student`, each with distinct permissions
- **JWT Authentication** — token-based auth with configurable expiry, issued at login
- **Secure Password Hashing** — BCrypt-based hashing, never stores plaintext passwords
- **Enrollment Management** — students can enroll in classes with seat-capacity enforcement
- **Class & Assignment Management** — CRUD operations scoped by role
- **Auto-seeded Admin Account** — a default admin user is seeded on first run for easy setup
- **Input Validation** — request validation via FluentValidation, with consistent error responses
- **Swagger / OpenAPI** — interactive API docs with built-in JWT bearer auth support
<img width="2720" height="2880" alt="class_assignment_system_architecture" src="https://github.com/user-attachments/assets/b179c823-0d89-4fed-8bae-f45a3e994004" />


## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core (.NET 10) |
| Architecture | Clean Architecture (Domain / Application / Infrastructure / API) |
| CQRS / Mediator | MediatR |
| Validation | FluentValidation |
| ORM | Entity Framework Core |
| Database | SQL Server |
| Auth | JWT Bearer Authentication |
| Password Hashing | BCrypt.Net-Next |
| API Docs | Swashbuckle (Swagger UI) |

## Project Structure

```
ClassAssignmentSystem/
├── src/
│   ├── ClassAssignmentSystem.Domain/          # Entities, enums, domain logic
│   ├── ClassAssignmentSystem.Application/     # CQRS commands/queries, DTOs, validators, interfaces
│   ├── ClassAssignmentSystem.Infrastructure/  # EF Core, repositories, external services
│   └── ClassAssignmentSystem.Api/             # Controllers, Program.cs, DI wiring
└── tests/
    └── ClassAssignmentSystem.Tests/           # Unit / integration tests
```

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB, Docker, or full instance)
- EF Core CLI tools:
  ```bash
  dotnet tool install --global dotnet-ef
  ```

### Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/<your-username>/ClassAssignmentSystem.git
   cd ClassAssignmentSystem
   ```

2. **Configure your connection string and JWT secret**

   Set them via .NET User Secrets (recommended for local dev — keeps secrets out of source control):
   ```bash
   cd src/ClassAssignmentSystem.Api
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=.;Database=ClassAssignmentSystemDb;Trusted_Connection=True;TrustServerCertificate=True;"
   dotnet user-secrets set "Jwt:Secret" "your-strong-secret-key-at-least-32-characters"
   ```

3. **Apply database migrations**
   ```bash
   dotnet ef database update --project src/ClassAssignmentSystem.Infrastructure --startup-project src/ClassAssignmentSystem.Api
   ```

4. **Run the API**
   ```bash
   dotnet run --project src/ClassAssignmentSystem.Api
   ```

5. **Open Swagger UI**

   Navigate to `https://localhost:<port>/swagger` to explore and test the API interactively.

### Default Admin Credentials

On first run, an admin account is auto-seeded so you can log in immediately:

```
Email: admin@classassignment.com
Password: (see seed configuration in Infrastructure layer)
```

> ⚠️ Change the seeded admin password before deploying anywhere beyond local development.

## Authentication

1. Call `POST /api/auth/login` with valid credentials to receive a JWT.
2. Click **Authorize** in Swagger UI and paste the token (no `Bearer` prefix needed — Swagger adds it automatically).
3. Authenticated requests will now include the token and respect role-based restrictions.

## Roles & Permissions

| Role | Permissions |
|---|---|
| **Admin** | Full access — manage users, classes, and enrollments |
| **Teacher** | Manage assigned classes and view enrolled students |
| **Student** | View available classes, enroll/unenroll (subject to seat capacity) |

## Running Tests

```bash
dotnet test
```

## Roadmap

- [ ] Redis caching for class listings
- [ ] Event-driven notifications via RabbitMQ / MassTransit
- [ ] Dockerized deployment
- [ ] CI/CD pipeline via GitHub Actions

## License

This project is available under the MIT License.

## Contact

Built by **Maria** — Senior Backend Software Engineer specializing in .NET / ASP.NET Core.
Feel free to connect on [LinkedIn]([https://linkedin.com](https://www.linkedin.com/in/imaria-arif/)) or reach out with questions.
