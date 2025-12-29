# 🐾 Pati Mahallem

Sokak hayvanlarını besleme, bakma ve takip etme platformu. 

## 🏗️ Architecture

Microservices tabanlı . NET 8 uygulaması.

### Services: 
- **User.Service** - Authentication & User Management
- **Point.Service** - Points & Subscriptions
- **Event.Service** - Events & Activities
- **Wallet.Service** - Transactions & Donations
- **Badge.Service** - Achievements & Badges
- **Notification.Service** - Push/Email/SMS

### Infrastructure:
- PostgreSQL - Database
- RabbitMQ - Message Broker
- Seq - Logging

## 🚀 Quick Start

```bash
# Infrastructure'ı başlat
docker-compose up -d

# Build
dotnet build

# User Service'i çalıştır
dotnet run --project src/Services/User.Service/User.Service.csproj