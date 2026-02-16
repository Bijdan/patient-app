# PatientApp - Clean Architecture Web API

A .NET 8 Web API for patient management, built following Clean Architecture principles with MongoDB as the data store.

---

## Table of Contents

- [Architecture Overview](#architecture-overview)
- [Patient Features](#patient-features)
- [Local Development Setup](#local-development-setup)
- [Adding a New Endpoint](#adding-a-new-endpoint)

---

## Architecture Overview

The solution follows **Clean Architecture** (also known as Onion Architecture), where dependencies flow strictly inward. The inner layers define abstractions; the outer layers provide implementations.

```
┌──────────────────────────────────────────────────┐
│  PatientApp.Api         (Presentation Layer)     │
│  Controllers, Program.cs, Swagger, Versioning    │
├──────────────────────────────────────────────────┤
│  PatientApp.Infrastructure   (Data Access Layer) │
│  MongoDB Repositories, DI, Settings, ClassMaps   │
├──────────────────────────────────────────────────┤
│  PatientApp.Application      (Use Case Layer)    │
│  Services, DTOs, Interfaces, Mappings            │
├──────────────────────────────────────────────────┤
│  PatientApp.Domain           (Core Layer)        │
│  Entities, Base Classes, Repository Interfaces   │
└──────────────────────────────────────────────────┘
```

### Project Dependency Graph

```
Api ──────► Application (interfaces, DTOs)
  └───────► Infrastructure (to call AddInfrastructure at startup)

Infrastructure ──► Application (registers services)
               └──► Domain (via transitive reference)
               └──► MongoDB.Driver (NuGet)

Application ──► Domain (entities, repository interfaces)

Domain ──► (no dependencies)
```

### Solution Structure

```
PatientApp/
├── PatientApp.sln
├── docs/
│   └── README.md
├── scripts/
│   ├── mongo-init.js              # DB + collection + indexes
│   └── mongo-seed.js              # Sample patient data
└── src/
    ├── PatientApp.Domain/         # Zero dependencies
    │   ├── Common/
    │   │   └── BaseEntity.cs      # Abstract base: Id, CreatedAt, UpdatedAt
    │   ├── Entities/
    │   │   └── Patient.cs         # Patient entity (pure POCO)
    │   └── Interfaces/
    │       └── IPatientRepository.cs
    ├── PatientApp.Application/    # References: Domain
    │   ├── DTOs/
    │   │   ├── PatientDto.cs
    │   │   ├── CreatePatientRequest.cs
    │   │   └── UpdatePatientRequest.cs
    │   ├── Interfaces/
    │   │   └── IPatientService.cs
    │   ├── Mappings/
    │   │   └── PatientMappingExtensions.cs
    │   └── Services/
    │       └── PatientService.cs
    ├── PatientApp.Infrastructure/  # References: Application | NuGet: MongoDB.Driver
    │   ├── DependencyInjection.cs  # Composition root (DI + BSON class maps)
    │   ├── Repositories/
    │   │   └── PatientRepository.cs
    │   └── Settings/
    │       └── MongoDbSettings.cs
    └── PatientApp.Api/             # References: Application, Infrastructure
        ├── Controllers/
        │   └── PatientsController.cs
        ├── Program.cs
        ├── appsettings.json
        └── Properties/
            └── launchSettings.json
```

### Key Design Decisions

| Decision | Rationale |
|----------|-----------|
| **Domain has zero dependencies** | Keeps the core model technology-agnostic. No MongoDB attributes on entities. |
| **BSON class maps in Infrastructure** | MongoDB serialization concerns (`BsonClassMap`) are configured in `DependencyInjection.cs`, not via `[BsonId]` attributes on domain entities. |
| **Manual mapping extensions** | `PatientMappingExtensions` uses plain C# extension methods instead of AutoMapper, avoiding a third-party dependency. |
| **URL-path API versioning** | Routes follow `api/v{version}/resource` pattern (e.g., `/api/v1/patients`) using `Asp.Versioning`. |
| **Repository pattern** | `IPatientRepository` is defined in Domain, implemented in Infrastructure, injected into Application services. |

---

## Patient Features

The API provides full CRUD operations for patient management, exposed under the versioned route `/api/v1/patients`.

### Endpoints

| Method   | Route                    | Description            | Request Body             | Success Response              |
|----------|--------------------------|------------------------|--------------------------|-------------------------------|
| `GET`    | `/api/v1/patients`       | List all patients      | —                        | `200 OK` with `PatientDto[]`  |
| `GET`    | `/api/v1/patients/{id}`  | Get patient by ID      | —                        | `200 OK` with `PatientDto`    |
| `POST`   | `/api/v1/patients`       | Create a new patient   | `CreatePatientRequest`   | `201 Created` with `PatientDto` |
| `PUT`    | `/api/v1/patients/{id}`  | Update an existing patient | `UpdatePatientRequest` | `200 OK` with `PatientDto`    |
| `DELETE` | `/api/v1/patients/{id}`  | Delete a patient       | —                        | `204 No Content`              |

All endpoints return `404 Not Found` when the specified patient ID does not exist (for GET by ID, PUT, DELETE).

### Patient Data Model

| Field         | Type       | Required | Notes                                      |
|---------------|------------|----------|---------------------------------------------|
| `Id`          | `string`   | Auto     | MongoDB ObjectId, auto-generated on insert  |
| `FirstName`   | `string`   | Yes      |                                             |
| `LastName`    | `string`   | Yes      |                                             |
| `DateOfBirth` | `DateTime` | Yes      |                                             |
| `Email`       | `string`   | Yes      | Unique index enforced at the database level |
| `Phone`       | `string?`  | No       | Nullable                                    |
| `CreatedAt`   | `DateTime` | Auto     | Set to UTC on creation                      |
| `UpdatedAt`   | `DateTime` | Auto     | Updated to UTC on every modification        |

### Example Request/Response

**Create a patient:**

```http
POST /api/v1/patients
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Doe",
  "dateOfBirth": "1985-03-15T00:00:00Z",
  "email": "john.doe@example.com",
  "phone": "+1-555-0101"
}
```

**Response (`201 Created`):**

```json
{
  "id": "65f1a2b3c4d5e6f7a8b9c0d1",
  "firstName": "John",
  "lastName": "Doe",
  "dateOfBirth": "1985-03-15T00:00:00Z",
  "email": "john.doe@example.com",
  "phone": "+1-555-0101",
  "createdAt": "2024-03-15T10:30:00Z",
  "updatedAt": "2024-03-15T10:30:00Z"
}
```

### MongoDB Indexes

Defined in `scripts/mongo-init.js`:

| Index          | Type    | Purpose                                  |
|----------------|---------|------------------------------------------|
| `Email`        | Unique  | Prevents duplicate patient email entries |
| `LastName`     | Regular | Supports efficient last-name queries     |
| `DateOfBirth`  | Regular | Supports date-range queries              |

---

## Local Development Setup

### Prerequisites

- **.NET 8 SDK** — [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **MongoDB** — Running locally on port `27017`

### 1. Install .NET 8 SDK

If not already installed:

```bash
# macOS / Linux
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 8.0

# Add to PATH (add to your shell profile for persistence)
export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$HOME/.dotnet:$PATH"

# Verify
dotnet --version   # Should output 8.x.x
```

### 2. Install and Start MongoDB

**macOS (Homebrew):**

```bash
brew tap mongodb/brew
brew install mongodb-community@7.0
brew services start mongodb-community@7.0
```

**Verify MongoDB is running:**

```bash
mongosh --eval "db.runCommand({ ping: 1 })"
```

### 3. Initialize the Database

```bash
# From the repository root
mongosh < scripts/mongo-init.js    # Creates database, collection, and indexes
mongosh < scripts/mongo-seed.js    # Inserts 5 sample patient records
```

### 4. Build and Run

```bash
# Restore and build
dotnet build PatientApp.sln

# Run the API
dotnet run --project src/PatientApp.Api
```

The API will start at **http://localhost:5002**.

### 5. Explore the API

- **Swagger UI:** http://localhost:5002/swagger
- **Test with curl:**

```bash
# List all patients
curl http://localhost:5002/api/v1/patients

# Get a specific patient
curl http://localhost:5002/api/v1/patients/{id}
```

### Configuration

MongoDB connection settings are in `src/PatientApp.Api/appsettings.json`:

```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "PatientDb"
  }
}
```

Override for specific environments using `appsettings.{Environment}.json` or environment variables:

```bash
export MongoDbSettings__ConnectionString="mongodb://myserver:27017"
export MongoDbSettings__DatabaseName="PatientDb_Dev"
```

### Debugging

**VS Code:** Use the C# Dev Kit extension. The `launchSettings.json` provides pre-configured profiles:
- `http` — Runs on `http://localhost:5002`, opens Swagger
- `https` — Runs on `https://localhost:7207`, opens Swagger

**Visual Studio:** Open `PatientApp.sln` and press `F5`. The `http` or `https` profile will launch with Swagger.

**Rider:** Open the solution and select the `PatientApp.Api` run configuration.

---

## Adding a New Endpoint

This guide walks through adding a new entity and its CRUD endpoints, using an `Appointment` as an example.

### Step 1: Domain Layer — Define the Entity and Repository Interface

**Create the entity** in `src/PatientApp.Domain/Entities/Appointment.cs`:

```csharp
using PatientApp.Domain.Common;

namespace PatientApp.Domain.Entities;

public class Appointment : BaseEntity
{
    public string PatientId { get; set; } = null!;
    public DateTime ScheduledAt { get; set; }
    public string Reason { get; set; } = null!;
    public string Status { get; set; } = null!; // e.g. "Scheduled", "Completed", "Cancelled"
}
```

**Create the repository interface** in `src/PatientApp.Domain/Interfaces/IAppointmentRepository.cs`:

```csharp
using PatientApp.Domain.Entities;

namespace PatientApp.Domain.Interfaces;

public interface IAppointmentRepository
{
    Task<IEnumerable<Appointment>> GetAllAsync();
    Task<Appointment?> GetByIdAsync(string id);
    Task CreateAsync(Appointment appointment);
    Task UpdateAsync(Appointment appointment);
    Task DeleteAsync(string id);
}
```

### Step 2: Application Layer — DTOs, Service Interface, and Implementation

**Create DTOs** in `src/PatientApp.Application/DTOs/`:

```csharp
// AppointmentDto.cs
public class AppointmentDto
{
    public string Id { get; set; } = null!;
    public string PatientId { get; set; } = null!;
    public DateTime ScheduledAt { get; set; }
    public string Reason { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// CreateAppointmentRequest.cs
public class CreateAppointmentRequest
{
    public string PatientId { get; set; } = null!;
    public DateTime ScheduledAt { get; set; }
    public string Reason { get; set; } = null!;
}
```

**Create the service interface** in `src/PatientApp.Application/Interfaces/IAppointmentService.cs`:

```csharp
public interface IAppointmentService
{
    Task<IEnumerable<AppointmentDto>> GetAllAsync();
    Task<AppointmentDto?> GetByIdAsync(string id);
    Task<AppointmentDto> CreateAsync(CreateAppointmentRequest request);
    Task<bool> DeleteAsync(string id);
}
```

**Create mapping extensions** in `src/PatientApp.Application/Mappings/AppointmentMappingExtensions.cs` following the pattern in `PatientMappingExtensions.cs`.

**Create the service** in `src/PatientApp.Application/Services/AppointmentService.cs` following the pattern in `PatientService.cs`.

### Step 3: Infrastructure Layer — Repository and Registration

**Create the repository** in `src/PatientApp.Infrastructure/Repositories/AppointmentRepository.cs`:

```csharp
using MongoDB.Driver;
using PatientApp.Domain.Entities;
using PatientApp.Domain.Interfaces;

namespace PatientApp.Infrastructure.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly IMongoCollection<Appointment> _collection;

    public AppointmentRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<Appointment>("Appointments");
    }

    // Implement all interface methods following PatientRepository as a template
}
```

**Register in DI** — Update `src/PatientApp.Infrastructure/DependencyInjection.cs`:

```csharp
// In AddInfrastructure method, add:
services.AddScoped<IAppointmentRepository, AppointmentRepository>();
services.AddScoped<IAppointmentService, AppointmentService>();

// In RegisterClassMaps method, add:
if (!BsonClassMap.IsClassMapRegistered(typeof(Appointment)))
{
    BsonClassMap.RegisterClassMap<Appointment>(cm =>
    {
        cm.AutoMap();
    });
}
```

### Step 4: API Layer — Controller

**Create the controller** in `src/PatientApp.Api/Controllers/AppointmentsController.cs`:

```csharp
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using PatientApp.Application.DTOs;
using PatientApp.Application.Interfaces;

namespace PatientApp.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetAll()
    {
        var appointments = await _appointmentService.GetAllAsync();
        return Ok(appointments);
    }

    // Add remaining actions (GetById, Create, Update, Delete)
    // following the same pattern as PatientsController
}
```

### Step 5: Database Setup (Optional)

If the new collection needs indexes or schema validation, add a script in `scripts/`:

```javascript
// scripts/mongo-init-appointments.js
const db = db.getSiblingDB("PatientDb");

db.createCollection("Appointments");
db.Appointments.createIndex({ PatientId: 1 });
db.Appointments.createIndex({ ScheduledAt: 1 });
```

### Step 6: Build and Verify

```bash
dotnet build PatientApp.sln
dotnet run --project src/PatientApp.Api
```

Open Swagger at `http://localhost:5002/swagger` to verify the new endpoints appear.

### Checklist

- [ ] Entity created in `Domain/Entities/`
- [ ] Repository interface created in `Domain/Interfaces/`
- [ ] DTOs created in `Application/DTOs/`
- [ ] Service interface created in `Application/Interfaces/`
- [ ] Mapping extensions created in `Application/Mappings/`
- [ ] Service implementation created in `Application/Services/`
- [ ] Repository implementation created in `Infrastructure/Repositories/`
- [ ] BSON class map registered in `Infrastructure/DependencyInjection.cs`
- [ ] DI registrations added in `Infrastructure/DependencyInjection.cs`
- [ ] Controller created in `Api/Controllers/`
- [ ] Solution builds with `dotnet build`
- [ ] Endpoints visible in Swagger
