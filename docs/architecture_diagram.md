# Microservice Architecture Diagram

## 1. High-Level System Architecture

```mermaid
graph TB
    subgraph "Client Layer"
        Mobile[📱 Mobile App]
        Web[🌐 Web App]
        Admin[⚙️ Admin Panel]
    end
    
    subgraph "Edge Layer"
        Gateway[🚪 API Gateway - YARP<br/>Port:  5000<br/>- JWT Auth<br/>- Rate Limiting<br/>- Routing]
    end
    
    subgraph "Application Layer - Microservices"
        UserService[👤 User Service<br/>Port: 5001<br/>- Auth/JWT<br/>- Users/Roles]
        PointService[📍 Point Service<br/>Port: 5002<br/>- Points<br/>- Subscriptions<br/>- Photos]
        EventService[📅 Event Service<br/>Port:  5003<br/>- Events<br/>- Event Types<br/>- Scheduler]
        WalletService[💰 Wallet Service<br/>Port: 5004<br/>- Wallets<br/>- Transactions<br/>- Donations]
        BadgeService[🏆 Badge Service<br/>Port: 5005<br/>- Badges<br/>- Achievements<br/>- Rewards]
        NotificationService[🔔 Notification Service<br/>Port: 5006<br/>- Push/Email/SMS<br/>- Outbox Pattern]
    end
    
    subgraph "Message Broker"
        RabbitMQ[🐰 RabbitMQ<br/>Port: 5672<br/>- Event Bus<br/>- MassTransit]
    end
    
    subgraph "Data Layer"
        PostgreSQL[(🐘 PostgreSQL<br/>Port: 5432<br/>- Main Database)]
        Redis[(⚡ Redis<br/>Port: 6379<br/>- Cache<br/>- Session)]
        S3[☁️ AWS S3<br/>- Photo Storage]
    end
    
    subgraph "Monitoring & Logging"
        Seq[📊 Seq<br/>Port: 5341<br/>- Centralized Logs]
        Prometheus[📈 Prometheus<br/>Port: 9090<br/>- Metrics]
        Grafana[📉 Grafana<br/>Port: 3000<br/>- Dashboards]
        Jaeger[🔍 Jaeger<br/>Port: 16686<br/>- Distributed Tracing]
    end
    
    Mobile --> Gateway
    Web --> Gateway
    Admin --> Gateway
    
    Gateway --> UserService
    Gateway --> PointService
    Gateway --> EventService
    Gateway --> WalletService
    Gateway --> BadgeService
    
    UserService --> PostgreSQL
    PointService --> PostgreSQL
    EventService --> PostgreSQL
    WalletService --> PostgreSQL
    BadgeService --> PostgreSQL
    NotificationService --> PostgreSQL
    
    UserService --> Redis
    PointService --> Redis
    EventService --> Redis
    
    PointService --> S3
    
    UserService -.->|Publish Events| RabbitMQ
    PointService -.->|Publish Events| RabbitMQ
    EventService -.->|Publish Events| RabbitMQ
    WalletService -.->|Publish Events| RabbitMQ
    BadgeService -.->|Publish Events| RabbitMQ
    
    RabbitMQ -.->|Consume Events| NotificationService
    RabbitMQ -.->|Consume Events| BadgeService
    RabbitMQ -.->|Consume Events| WalletService
    
    UserService --> Seq
    PointService --> Seq
    EventService --> Seq
    WalletService --> Seq
    BadgeService --> Seq
    NotificationService --> Seq
    
    UserService --> Prometheus
    PointService --> Prometheus
    EventService --> Prometheus
    WalletService --> Prometheus
    BadgeService --> Prometheus
    NotificationService --> Prometheus
    
    Prometheus --> Grafana
    
    UserService --> Jaeger
    PointService --> Jaeger
    EventService --> Jaeger
    WalletService --> Jaeger
    BadgeService --> Jaeger
    NotificationService --> Jaeger
    
    style Gateway fill:#ff6b6b,stroke:#c92a2a,color:#fff
    style RabbitMQ fill:#ff922b,stroke:#e67700,color:#fff
    style PostgreSQL fill:#4dabf7,stroke:#1971c2,color:#fff
    style Redis fill:#ff6b6b,stroke:#c92a2a,color:#fff
```

## 2. Event-Driven Flow Example:  User Creates Event

```mermaid
sequenceDiagram
    actor User as 👤 Mobile User
    participant Gateway as 🚪 API Gateway
    participant EventSvc as 📅 Event Service
    participant DB as 🐘 PostgreSQL
    participant RMQ as 🐰 RabbitMQ
    participant NotifSvc as 🔔 Notification Service
    participant BadgeSvc as 🏆 Badge Service
    participant OutboxDB as 📬 Outbox Table
    
    User->>Gateway: POST /api/events<br/>{point_id, event_type_id}
    Gateway->>Gateway: Validate JWT
    Gateway->>EventSvc: Forward Request
    
    EventSvc->>EventSvc:  Validate Business Rules
    EventSvc->>DB: BEGIN TRANSACTION
    EventSvc->>DB: INSERT INTO events
    EventSvc->>DB:  COMMIT
    
    EventSvc->>RMQ:  Publish "event. created"<br/>{event_id, point_id, user_id}
    EventSvc-->>Gateway: 201 Created
    Gateway-->>User: Success Response
    
    RMQ->>NotifSvc: Consume "event.created"
    NotifSvc->>DB: Get point subscribers
    NotifSvc->>OutboxDB: INSERT notifications<br/>(status:  pending)
    NotifSvc->>NotifSvc: Background Worker
    NotifSvc->>NotifSvc: Send Push Notifications
    NotifSvc->>OutboxDB: UPDATE status = 'sent'
    NotifSvc->>RMQ: ACK Message
    
    RMQ->>BadgeSvc: Consume "event.created"
    BadgeSvc->>DB: Count user's events
    BadgeSvc->>DB: Check badge requirements
    alt Badge Earned
        BadgeSvc->>DB: INSERT user_badges
        BadgeSvc->>RMQ: Publish "badge.earned"
        RMQ->>NotifSvc:  Consume "badge.earned"
        NotifSvc->>NotifSvc: Send Badge Notification
    end
    BadgeSvc->>RMQ: ACK Message
```

## 3. Donation Flow with Saga Pattern

```mermaid
sequenceDiagram
    actor Donor as 👤 Donor
    participant Gateway as 🚪 API Gateway
    participant WalletSvc as 💰 Wallet Service
    participant DB as 🐘 PostgreSQL
    participant SagaDB as 📋 Saga State
    participant RMQ as 🐰 RabbitMQ
    participant NotifSvc as 🔔 Notification
    
    Donor->>Gateway: POST /api/donations<br/>{recipient_id, amount, currency}
    Gateway->>WalletSvc: Forward Request
    
    WalletSvc->>SagaDB: Create Saga Instance<br/>(status: started)
    
    rect rgb(200, 255, 200)
        Note over WalletSvc,DB: Step 1: Deduct from Donor
        WalletSvc->>DB:  BEGIN TRANSACTION
        WalletSvc->>DB: UPDATE wallet_balances<br/>donor -= amount
        WalletSvc->>SagaDB: Update Step 1: completed
        WalletSvc->>DB: COMMIT
    end
    
    rect rgb(200, 255, 200)
        Note over WalletSvc,DB: Step 2: Add to Recipient
        WalletSvc->>DB: BEGIN TRANSACTION
        WalletSvc->>DB: UPDATE wallet_balances<br/>recipient += amount
        WalletSvc->>SagaDB: Update Step 2: completed
        WalletSvc->>DB: COMMIT
    end
    
    rect rgb(200, 255, 200)
        Note over WalletSvc,DB: Step 3: Record Donation
        WalletSvc->>DB: BEGIN TRANSACTION
        WalletSvc->>DB: INSERT INTO donations
        WalletSvc->>SagaDB: Update Step 3: completed
        WalletSvc->>DB:  COMMIT
    end
    
    WalletSvc->>SagaDB: Update Saga:  completed
    WalletSvc->>RMQ: Publish "donation.completed"
    WalletSvc-->>Gateway: 201 Created
    Gateway-->>Donor: Success
    
    RMQ->>NotifSvc: Consume "donation.completed"
    NotifSvc->>NotifSvc: Send notifications to<br/>donor & recipient
    
    alt Step 2 Fails - Rollback
        rect rgb(255, 200, 200)
            Note over WalletSvc,DB:  Compensation:  Rollback Step 1
            WalletSvc->>SagaDB: Update status:  compensating
            WalletSvc->>DB: BEGIN TRANSACTION
            WalletSvc->>DB: UPDATE wallet_balances<br/>donor += amount (refund)
            WalletSvc->>DB:  COMMIT
            WalletSvc->>SagaDB: Update status: failed
            WalletSvc-->>Gateway: 500 Error
        end
    end
```

## 4. Notification Service - Outbox Pattern

```mermaid
graph TB
    subgraph "Notification Service"
        Consumer[🔔 Event Consumer]
        OutboxWriter[📝 Outbox Writer]
        BackgroundWorker[⏰ Background Worker<br/>Every 10 seconds]
        Sender[📤 Notification Sender]
    end
    
    subgraph "Database"
        OutboxTable[(📬 notification_outbox<br/>- pending<br/>- sent<br/>- failed)]
    end
    
    subgraph "External Services"
        Firebase[🔥 Firebase FCM<br/>Push Notifications]
        SendGrid[📧 SendGrid<br/>Email]
        Twilio[📱 Twilio<br/>SMS]
    end
    
    RabbitMQ[🐰 RabbitMQ] -->|1.  Consume Event| Consumer
    Consumer -->|2. Insert| OutboxWriter
    OutboxWriter -->|3. Save| OutboxTable
    Consumer -->|4. ACK| RabbitMQ
    
    BackgroundWorker -->|5. SELECT pending/failed| OutboxTable
    BackgroundWorker -->|6. Process| Sender
    
    Sender -->|7a. Send Push| Firebase
    Sender -->|7b. Send Email| SendGrid
    Sender -->|7c. Send SMS| Twilio
    
    Sender -->|8a. Success| OutboxTable
    Sender -->|8b. Failed + Retry| OutboxTable
    
    style OutboxTable fill:#4dabf7,stroke:#1971c2,color:#fff
    style BackgroundWorker fill:#ff922b,stroke:#e67700,color:#fff
    style Sender fill:#51cf66,stroke:#2f9e44,color:#fff
```

## 5. Service Communication Patterns

```mermaid
graph LR
    subgraph "Synchronous Communication"
        Gateway[API Gateway]
        Gateway -->|HTTP/REST| UserSvc[User Service]
        Gateway -->|HTTP/REST| PointSvc[Point Service]
        Gateway -->|HTTP/REST| EventSvc[Event Service]
        
        EventSvc -.->|HTTP with Polly<br/>Retry + Circuit Breaker| UserSvc
        WalletSvc -.->|HTTP with Polly| BadgeSvc[Badge Service]
    end
    
    subgraph "Asynchronous Communication"
        EventSvc2[Event Service]
        PointSvc2[Point Service]
        WalletSvc2[Wallet Service]
        
        EventSvc2 -->|Publish| RMQ[RabbitMQ]
        PointSvc2 -->|Publish| RMQ
        WalletSvc2 -->|Publish| RMQ
        
        RMQ -->|Subscribe| NotifSvc[Notification Service]
        RMQ -->|Subscribe| BadgeSvc2[Badge Service]
        RMQ -->|Subscribe| WalletSvc3[Wallet Service]
    end
    
    style RMQ fill:#ff922b,stroke:#e67700,color:#fff
```

## 6. Database Schema Distribution

```mermaid
graph TB
    subgraph "PostgreSQL - Shared Database"
        subgraph "User Domain"
            UsersTable[users]
            RolesTable[roles]
            UserRolesTable[user_roles]
        end
        
        subgraph "Point Domain"
            PointsTable[points]
            PointPhotosTable[point_photos]
            PointSubsTable[point_subscriptions]
        end
        
        subgraph "Event Domain"
            EventsTable[events]
            EventTypesTable[event_types]
        end
        
        subgraph "Wallet Domain"
            WalletsTable[wallets]
            WalletBalancesTable[wallet_balances]
            TransactionsTable[transactions]
            DonationsTable[donations]
            CurrenciesTable[currencies]
            CurrencyPricesTable[currency_prices]
        end
        
        subgraph "Badge Domain"
            BadgesTable[badges]
            UserBadgesTable[user_badges]
            BadgeCurrReqTable[badge_currency_requirements]
            BadgeEventReqTable[badge_event_requirements]
        end
        
        subgraph "Infrastructure"
            SagaTable[saga_instances]
            SagaStepsTable[saga_steps]
            OutboxTable[notification_outbox]
        end
    end
    
    UserService[User Service] -.->|Read/Write| UsersTable
    UserService -.->|Read/Write| RolesTable
    
    PointService[Point Service] -.->|Read/Write| PointsTable
    PointService -.->|Read/Write| PointSubsTable
    
    EventService[Event Service] -.->|Read/Write| EventsTable
    
    WalletService[Wallet Service] -.->|Read/Write| WalletsTable
    WalletService -.->|Read/Write| TransactionsTable
    
    BadgeService[Badge Service] -.->|Read/Write| BadgesTable
    BadgeService -.->|Read| EventsTable
    
    NotificationService[Notification Service] -.->|Read/Write| OutboxTable
    
    style UsersTable fill:#4dabf7,stroke:#1971c2,color:#fff
    style PointsTable fill:#4dabf7,stroke:#1971c2,color:#fff
    style EventsTable fill:#4dabf7,stroke:#1971c2,color:#fff
    style WalletsTable fill:#4dabf7,stroke:#1971c2,color:#fff
    style BadgesTable fill:#4dabf7,stroke:#1971c2,color:#fff
```

## 7. Deployment Architecture (Docker Compose - Development)

```mermaid
graph TB
    subgraph "Docker Network:  pati-network"
        subgraph "Services"
            Gateway[api-gateway<br/>Port: 5000]
            UserSvc[user-service<br/>Port: 5001]
            PointSvc[point-service<br/>Port: 5002]
            EventSvc[event-service<br/>Port: 5003]
            WalletSvc[wallet-service<br/>Port: 5004]
            BadgeSvc[badge-service<br/>Port: 5005]
            NotifSvc[notification-service<br/>Port: 5006]
        end
        
        subgraph "Infrastructure"
            Postgres[postgres<br/>Port: 5432]
            Redis[redis<br/>Port: 6379]
            RabbitMQ[rabbitmq<br/>Port: 5672, 15672]
            Seq[seq<br/>Port: 5341]
            Prometheus[prometheus<br/>Port: 9090]
            Grafana[grafana<br/>Port: 3000]
            Jaeger[jaeger<br/>Port:  16686]
        end
    end
    
    Gateway --> UserSvc
    Gateway --> PointSvc
    Gateway --> EventSvc
    Gateway --> WalletSvc
    Gateway --> BadgeSvc
    
    UserSvc --> Postgres
    PointSvc --> Postgres
    EventSvc --> Postgres
    WalletSvc --> Postgres
    BadgeSvc --> Postgres
    NotifSvc --> Postgres
    
    UserSvc --> Redis
    PointSvc --> Redis
    
    UserSvc --> RabbitMQ
    EventSvc --> RabbitMQ
    WalletSvc --> RabbitMQ
    NotifSvc --> RabbitMQ
    BadgeSvc --> RabbitMQ
    
    UserSvc --> Seq
    EventSvc --> Seq
    NotifSvc --> Seq
    
    UserSvc --> Prometheus
    EventSvc --> Prometheus
    
    UserSvc --> Jaeger
    EventSvc --> Jaeger
    
    style Gateway fill:#ff6b6b,stroke:#c92a2a,color:#fff
    style Postgres fill:#4dabf7,stroke:#1971c2,color:#fff
    style RabbitMQ fill:#ff922b,stroke:#e67700,color:#fff
```

## 8. Resilience Patterns

```mermaid
graph TB
    subgraph "Client Request Flow with Resilience"
        Client[📱 Client]
        Gateway[🚪 API Gateway]
        
        subgraph "Event Service"
            Controller[Controller]
            Polly{Polly Policies}
            HttpClient[HTTP Client]
        end
        
        subgraph "User Service"
            UserAPI[User API]
        end
        
        Client -->|1. Request| Gateway
        Gateway -->|2. Forward| Controller
        Controller -->|3. Call User Service| Polly
        
        Polly -->|Timeout Policy<br/>10 seconds| TimeoutCheck{Timeout? }
        TimeoutCheck -->|No| Polly
        TimeoutCheck -->|Yes| TimeoutError[❌ Timeout Error]
        
        Polly -->|Retry Policy<br/>3 attempts<br/>Exponential Backoff| RetryCheck{Retry?}
        RetryCheck -->|Attempt| HttpClient
        RetryCheck -->|Max Attempts| RetryError[❌ Retry Failed]
        
        HttpClient -->|4. HTTP Request| CircuitBreaker{Circuit Breaker}
        CircuitBreaker -->|Closed| UserAPI
        CircuitBreaker -->|Open<br/>5 failures| CircuitError[❌ Circuit Open]
        
        UserAPI -->|5. Response| HttpClient
        HttpClient -->|6. Success| Controller
        Controller -->|7. Response| Gateway
        Gateway -->|8. Response| Client
        
        TimeoutError --> Fallback[🔄 Fallback Response]
        RetryError --> Fallback
        CircuitError --> Fallback
        Fallback --> Controller
    end
    
    style Polly fill:#ff922b,stroke:#e67700,color:#fff
    style CircuitBreaker fill:#ff6b6b,stroke:#c92a2a,color:#fff
    style Fallback fill:#51cf66,stroke:#2f9e44,color:#fff
```

## 9. Monitoring & Observability

```mermaid
graph TB
    subgraph "Application Layer"
        Services[All Microservices]
    end
    
    subgraph "Observability Stack"
        subgraph "Logging"
            Serilog[Serilog]
            Seq[Seq - Log Aggregation]
        end
        
        subgraph "Metrics"
            PromClient[Prometheus Client]
            Prometheus[Prometheus]
            Grafana[Grafana Dashboards]
        end
        
        subgraph "Tracing"
            OTel[OpenTelemetry]
            Jaeger[Jaeger]
        end
        
        subgraph "Health Checks"
            HealthEndpoint["health endpoint"]
            HealthUI[Health Checks UI]
        end
    end
    
    Services -->|Structured Logs| Serilog
    Serilog -->|Ship Logs| Seq
    
    Services -->|Expose /metrics| PromClient
    PromClient -->|Scrape| Prometheus
    Prometheus -->|Visualize| Grafana
    
    Services -->|Trace Context| OTel
    OTel -->|Export Spans| Jaeger
    
    Services -->|Expose /health| HealthEndpoint
    HealthEndpoint -->|Monitor| HealthUI
    
    Grafana -.->|Alerts| Slack["📢 Slack/Email"]
    
    style Seq fill:#ff922b,stroke:#e67700,color:#fff
    style Prometheus fill:#e03131,stroke:#c92a2a,color:#fff
    style Grafana fill:#ff6b6b,stroke:#e67700,color:#fff
    style Jaeger fill:#4dabf7,stroke:#1971c2,color:#fff
```
```