# 🐾 Pati Mahallem

A street animal feeding, tracking, and management platform. 

## 📋 Table of Contents

- [Architecture](#-architecture)
- [Features](#-features)
- [Tech Stack](#️-tech-stack)
- [Getting Started](#-getting-started)
- [Services](#-services)
- [API Usage](#-api-usage)
- [Monitoring](#-monitoring)
- [Development](#-development)
- [Roadmap](#-roadmap)

---

## 🏗️ Architecture

Event-driven microservices architecture built with .NET 8.

```
┌─────────────────────────────────────────────────────────┐
│                    Client Layer                         │
│   📱 Mobile App    🌐 Web App    ⚙️ Admin Panel        │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│                  🚪 API Gateway (YARP)                  │
│           JWT Auth • Rate Limiting • Routing            │
└─────┬────────┬────────┬────────┬────────┬──────────────┘
      │        │        │        │        │
┌─────▼──┐ ┌──▼────┐ ┌─▼─────┐ ┌▼──────┐ ┌▼────────┐
│  User  │ │ Point │ │ Event │ │Wallet │ │  Badge  │
│Service │ │Service│ │Service│ │Service│ │ Service │
└───┬────┘ └───┬───┘ └───┬───┘ └───┬───┘ └────┬────┘
    │          │         │         │          │
    └──────────┴─────────┴─────────┴──────────┘
                     │
        ┌────────────▼───────────────┐
        │    🐰 RabbitMQ (Events)    │
        └────────────┬───────────────┘
                     │
                ┌────▼─────┐
                │Notification│
                │  Service   │
                └────┬───────┘
                     │
    ┌────────────────┼────────────────┐
    │                │                │
┌───▼──────┐  ┌──────▼─────┐  ┌──────▼──────┐
│PostgreSQL│  │  RabbitMQ  │  │     Seq     │
│   16     │  │    3.13    │  │   Logging   │
└──────────┘  └────────────┘  └─────────────┘
```

Detailed architecture:  [Architecture Diagram](docs/architecture-diagram.md)

---

## ✨ Features

### ✅ Completed (User Service)

- 🔐 **JWT Authentication** - Token-based authentication
- 👤 **User Management** - Registration, login, profile
- 🎭 **Role Management** - Admin, Caretaker, Donor
- 🔒 **Password Security** - BCrypt hashing
- 📨 **Event Publishing** - Async communication via RabbitMQ
- 📊 **Logging** - Centralized logging with Seq
- 📚 **API Documentation** - Swagger/OpenAPI

### 🚧 In Development

- 📍 **Point Service** - Animal point management
- 📅 **Event Service** - Activity tracking
- 💰 **Wallet Service** - Wallet and donations
- 🏆 **Badge Service** - Achievement system
- 🔔 **Notification Service** - Push, Email, SMS
- 🚪 **API Gateway** - Single entry point

---

## 🛠️ Tech Stack

### Backend

- **Framework:** .NET 8
- **ORM:** Entity Framework Core 8
- **Database:** PostgreSQL 16
- **Message Broker:** RabbitMQ 3.13 + MassTransit
- **Authentication:** JWT Bearer
- **Logging:** Serilog + Seq
- **API Documentation:** Swagger/OpenAPI
- **Password Hashing:** BCrypt. NET

### Infrastructure

- **Containerization:** Docker & Docker Compose
- **Monitoring:** Seq (Centralized Logging)
- **Message Queue UI:** RabbitMQ Management

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Git](https://git-scm.com/)

### Installation

```bash
# Clone the repository
git clone https://github.com/demirdanis/patimahallem.git
cd patimahallem

# Start infrastructure (PostgreSQL, RabbitMQ, Seq)
docker-compose up -d

# Verify containers are running
docker-compose ps
```

### Run User Service

```bash
# Navigate to User Service directory
cd src/Services/User.Service

# Apply database migrations
dotnet ef database update

# Run the service
dotnet run

# Open Swagger in another terminal
open http://localhost:5178/swagger
```

---

## 📦 Services

### 1. 👤 User Service (Port: 5178)

**Status:** ✅ Completed

**Features:**
- User registration
- User login
- JWT token generation
- Role-based authorization
- Profile information (/me endpoint)

**Endpoints:**
```
POST   /api/auth/register  - Register new user
POST   /api/auth/login     - User login
GET    /api/auth/me        - Profile info (🔒 JWT required)
GET    /health             - Health check
```

**Roles:**
- `admin` - System administrator
- `pati_bakici` - Animal caretaker
- `bagisci` - Donor

---

### 2. 📍 Point Service (Port: 5002)

**Status:** 📋 Planned

**Features:**
- Create animal points
- Photo upload (AWS S3)
- Point subscriptions
- Location-based search

---

### 3. 📅 Event Service (Port: 5003)

**Status:** 📋 Planned

**Features:**
- Activity logging (feeding, vaccination, etc.)
- Recurring events
- Event scheduler
- Tracking system

---

### 4. 💰 Wallet Service (Port: 5004)

**Status:** 📋 Planned

**Features:**
- Wallet management
- Virtual currency
- Donation system (Saga Pattern)
- Transaction history

---

### 5. 🏆 Badge Service (Port: 5005)

**Status:** 📋 Planned

**Features:**
- Achievement badge system
- Automatic rank calculation
- User statistics

---

### 6. 🔔 Notification Service (Port: 5006)

**Status:** 📋 Planned

**Features:**
- Push notifications (Firebase FCM)
- Email (SendGrid)
- SMS (Twilio)
- Guaranteed delivery with Outbox Pattern

---

## 🧪 API Usage

### 1. User Registration

```bash
curl -X POST http://localhost:5178/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john@example.com",
    "password":  "SecurePass123!",
    "fullName": "John Doe",
    "phone": "+15551234567"
  }'
```

**Response (201 Created):**
```json
{
  "userId": 1,
  "email": "john@example.com",
  "fullName": "John Doe",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.. .",
  "roles": ["bagisci"]
}
```

---

### 2. User Login

```bash
curl -X POST http://localhost:5178/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john@example.com",
    "password":  "SecurePass123!"
  }'
```

**Response (200 OK):**
```json
{
  "userId": 1,
  "email": "john@example.com",
  "fullName": "John Doe",
  "token":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "roles": ["bagisci"]
}
```

---

### 3. Get Profile (with JWT)

```bash
# Save token to variable
TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."

curl -X GET http://localhost:5178/api/auth/me \
  -H "Authorization: Bearer $TOKEN"
```

**Response (200 OK):**
```json
{
  "userId":  1,
  "email":  "john@example.com",
  "fullName": "John Doe",
  "phone": "+15551234567",
  "isActive": true,
  "roles":  ["bagisci"],
  "createdAt": "2025-12-29T20:00:00Z"
}
```

---

## 📊 Monitoring

### 🐰 RabbitMQ Management UI

**URL:** http://localhost:15672  
**Login:** `guest` / `guest`

**Features:**
- 📨 **Exchanges** - Published events
- 📬 **Queues** - Consumer queues
- 🔌 **Connections** - Active service connections
- 📊 **Channels** - Message channels

---

### 📊 Seq - Centralized Logging

**URL:** http://localhost:5341  
**Login:** `admin` / `Admin123! `

**Filters:**
```
Service = "User. Service"                    # Filter by service
Level = "Error"                             # Show only errors
UserId = 123                                # Filter by user
@Message like '%registered%'                # Search in message
```

**Example Query:**
```
Service = "User.Service" and Level = "Information"
```

---

### 🐘 PostgreSQL Database

**Connection Details:**
```
Host:  localhost
Port: 5432
Database: patimahallem
Username: postgres
Password: postgres
```

**Connect via CLI:**
```bash
# Connect to container
docker exec -it patimahallem-postgres psql -U postgres -d patimahallem

# List tables
\dt

# View users
SELECT id, email, full_name, is_active FROM users;

# View roles
SELECT * FROM roles;

# Exit
\q
```

---

## 👨‍💻 Development

### Project Structure

```
patimahallem/
├── docs/                           # Documentation
│   ├── architecture-diagram.md     # Architecture diagrams
│   └── database-schema.md          # Database schema
├── src/
│   ├── ApiGateway/                 # YARP API Gateway
│   ├── Services/                   # Microservices
│   │   ├── User.Service/          # ✅ Authentication
│   │   ├── Point.Service/         # 📋 Points
│   │   ├── Event.Service/         # 📋 Events
│   │   ├── Wallet. Service/        # 📋 Wallet
│   │   ├── Badge.Service/         # 📋 Badges
│   │   └── Notification.Service/  # 📋 Notifications
│   └── Shared/                     # Shared libraries
│       ├── Shared. Contracts/       # DTOs & Events
│       ├── Shared.Domain/          # Domain entities
│       └── Shared. Infrastructure/  # Common utilities
├── tests/
│   ├── Unit. Tests/                 # Unit tests
│   └── Integration.Tests/          # Integration tests
├── docker-compose.yml              # Infrastructure
└── PatiMahallem.sln               # Solution file
```

---

### Adding New Migration

```bash
cd src/Services/User.Service

# Create migration
dotnet ef migrations add MigrationName

# Apply to database
dotnet ef database update

# Remove last migration
dotnet ef migrations remove
```

---

### Adding New Event

1. **Define event** (`Shared. Contracts/Events/`):
```csharp
public record EventCreated
{
    public long EventId { get; init; }
    public long PointId { get; init; }
    public DateTime CreatedAt { get; init; }
}
```

2. **Publisher** (Event Service):
```csharp
await _publishEndpoint. Publish(new EventCreated { ...  });
```

3. **Consumer** (Notification Service):
```csharp
public class EventCreatedConsumer : IConsumer<EventCreated>
{
    public async Task Consume(ConsumeContext<EventCreated> context)
    {
        // Handle event
    }
}
```

---

### Testing

```bash
# Run all tests
dotnet test

# Unit tests only
dotnet test tests/Unit.Tests

# Integration tests only
dotnet test tests/Integration.Tests

# With coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

---

## 📈 Roadmap

### Phase 1: Foundation ✅
- [x] Project structure
- [x] Docker Compose infrastructure
- [x] Shared libraries
- [x] User Service with JWT authentication

### Phase 2: Core Services 🚧
- [ ] Point Service (points, photos, subscriptions)
- [ ] Event Service (activities, scheduler)
- [ ] Wallet Service (transactions, donations)

### Phase 3: Advanced Features 📋
- [ ] Badge Service (achievements)
- [ ] Notification Service (push, email, SMS)
- [ ] API Gateway (YARP)

### Phase 4: Quality & Deploy 📋
- [ ] Unit & Integration tests
- [ ] CI/CD pipeline (GitHub Actions)
- [ ] Kubernetes deployment
- [ ] Production monitoring (Prometheus, Grafana)

---

## 🤝 Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'feat: Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Commit Convention

```
feat: New feature
fix: Bug fix
docs: Documentation
chore: Configuration, dependencies
refactor: Code refactoring
test: Adding/fixing tests
```

---

## 📄 License

This project is licensed under the MIT License.

---

## 👨‍💻 Developer

**Demir Daniş**

- GitHub: [@demirdanis](https://github.com/demirdanis)
- Email: demirdanis@gmail. com

---

## 🙏 Acknowledgments

This project is developed to help street animals. 

**Contribute to make a difference in a furry friend's life!** 🐾

---

## 📞 Contact

For questions: 
- 📧 Email: demirdanis@gmail.com
- 🐛 Issues: [GitHub Issues](https://github.com/demirdanis/patimahallem/issues)
- 💬 Discussions: [GitHub Discussions](https://github.com/demirdanis/patimahallem/discussions)