#!/bin/bash

echo "📦 Adding NuGet packages to all services..."

SERVICES=(
    "User.Service"
    "Point.Service"
    "Event.Service"
    "Wallet.Service"
    "Badge.Service"
    "Notification.Service"
)

for service in "${SERVICES[@]}"; do
    echo "➕ Adding packages to $service..."
    cd "src/Services/$service"
    
    dotnet add package Npgsql.EntityFrameworkCore. PostgreSQL --version 8.0.0
    dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.0
    dotnet add package Microsoft. AspNetCore.Authentication.JwtBearer --version 8.0.0
    dotnet add package MassTransit. RabbitMQ --version 8.1.3
    dotnet add package Serilog.AspNetCore --version 8.0.0
    dotnet add package Serilog. Sinks. Seq --version 6.0.0
    dotnet add package Swashbuckle.AspNetCore --version 6.5.0
    
    cd ../../..
done

echo "✅ All packages added!"
dotnet restore
dotnet build
