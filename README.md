# LogicLink API

The backend for **LogicLink** — a real-time collaborative logic circuit builder. Built with ASP.NET Core, this API handles circuit persistence, live multi-user collaboration over SignalR, boolean circuit simulation, truth table generation, and PDF export.

No authentication — anyone with a circuit link can join and edit it live.

## Tech stack

- **.NET 8** / ASP.NET Core Web API
- **Entity Framework Core 8** with **PostgreSQL** (Npgsql)
- **SignalR** for real-time collaboration
- **QuestPDF** + **SkiaSharp** for circuit-diagram PDF export
- **FluentValidation** for request validation
- **Swashbuckle (Swagger)** for API docs in Development

## Features

- Create, list, update, soft-delete, restore, and permanently delete circuits
- Trash view with 30-day-style soft delete (`DeletedAt` tracked on the entity)
- Real-time gate/wire editing synced across every connected client via SignalR
- Live cursor position broadcasting per circuit room
- Boolean circuit simulation with manual input overrides, including feedback-loop detection
- Automatic truth table generation from the current circuit graph
- Circuit diagram rendered and exported as a PDF

## Project structure

LogicLink.Api/
├── Controllers/ # CircuitsController, SimulationController, ExportController
├── Hubs/ # CircuitHub — SignalR real-time entry point
├── Realtime/ # PresenceTracker — per-circuit connection/participant state
├── Services/ # CircuitService, CircuitSimulationService, PdfExportService
├── Models/
│ ├── Entities/ # Circuit, Gate, Wire
│ └── Enums/ # GateType
├── DTOs/ # Request/response contracts per feature
├── Validators/ # FluentValidation rules
├── Middleware/ # Global exception handling
├── Extensions/ # Middleware + SkiaSharp/QuestPDF drawing helpers
├── Data/ # AppDbContext
└── Program.cs


## Getting started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- PostgreSQL 16 (or Docker, see below)
- EF Core CLI tools: `dotnet tool install --global dotnet-ef`

### 1. Configure the connection string

Set `ConnectionStrings:DefaultConnection` in `appsettings.Development.json` (or as an environment variable), for example:

Host=localhost;Port=5432;Database=logiclink;Username=postgres;Password=postgres


Also set `Cors:AllowedOrigins` to include the frontend's dev URL, e.g. `["http://localhost:5173"]`.

### 2. Create and apply the database migration

This repo does not include a `Migrations` folder yet — generate one before the first run, otherwise the app will fail on startup (`Program.cs` calls `Database.MigrateAsync()` on boot):

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 3. Run the API

```bash
dotnet restore
dotnet run
```

By default the API listens on `http://localhost:8080` (see `docker-compose.yml`). In Development, Swagger UI is available at `/swagger`.

### Running with Docker Compose instead

```bash
docker-compose up --build
```

This starts PostgreSQL and the API together (API on `:8080`, Postgres on `:5432`).

## API reference

| Method | Route | Description |
|---|---|---|
| GET | `/api/circuits` | List all active circuits |
| GET | `/api/circuits/trash` | List soft-deleted circuits |
| GET | `/api/circuits/{id}` | Get full circuit detail (gates + wires) |
| POST | `/api/circuits` | Create a new circuit |
| PUT | `/api/circuits/{id}/settings` | Update name, grid size, snap-to-grid, show-labels |
| DELETE | `/api/circuits/{id}` | Soft-delete (move to trash) |
| POST | `/api/circuits/{id}/restore` | Restore from trash |
| DELETE | `/api/circuits/{id}/permanent` | Permanently delete |
| POST | `/api/circuits/{id}/simulate` | Run simulation with input overrides |
| GET | `/api/circuits/{id}/truth-table` | Generate the truth table |
| GET | `/api/circuits/{id}/export/pdf` | Export the circuit diagram as a PDF |

## Real-time hub — `/hubs/circuit`

**Client → Server methods**

| Method | Purpose |
|---|---|
| `JoinCircuit(circuitId, displayName)` | Join a circuit's room; returns assigned name/color + current participants |
| `MoveCursor(circuitId, x, y)` | Broadcast live cursor position |
| `AddGate(circuitId, gate)` | Persist and broadcast a new gate |
| `MoveGate(circuitId, request)` | Persist and broadcast a gate's new position |
| `DeleteGate(circuitId, gateId)` | Delete and broadcast gate removal |
| `AddWire(circuitId, wire)` | Persist and broadcast a new wire |
| `DeleteWire(circuitId, wireId)` | Delete and broadcast wire removal |

**Server → Client events**

`UserJoined`, `UserLeft`, `CursorMoved`, `GateAdded`, `GateMoved`, `GateDeleted`, `WireAdded`, `WireDeleted`

## Notes

- There is no authentication layer — a display name is the only identity, and name collisions inside a room are resolved automatically by `PresenceTracker`.
- `Simulate` and `GetTruthTable` return `409 Conflict` with a `{ message }` body when the circuit graph contains a feedback loop.