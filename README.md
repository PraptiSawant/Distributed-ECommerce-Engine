# README.md
## Event-Driven ECommerce Engine (.NET 9)
A high-performance, cloud-native distributed e-commerce backend built with an event-driven architecture to handle high-concurrency order placement and prevent warehouse over-selling race conditions.
## 🏛️ Architectural Overview
This ecosystem is designed using Clean Architecture (Onion Architecture) patterns to strictly enforce boundaries between core domain rules, application use cases, and external infrastructure framework details.
## 🔄 The System Topology

* ECommerce.Domain: The core layer. Contains pure domain entities encapsulated with strict business rules (e.g., preventing negative inventory values). Has zero external framework dependencies.
* ECommerce.Application: Coordinates system functionality using the CQRS (Command Query Responsibility Segregation) pattern via MediatR. Splits read-heavy catalog parsing from transactional commands.
* ECommerce.Infrastructure: Handles boundary data mapping. Contains data seeding engines and database schema setups.
* ECommerce.API: A high-performance, ultra-thin ASP.NET Core Minimal API wrapper tracking system route end-points.
* ECommerce.InventoryWorker: A dedicated, non-blocking asynchronous background service that pulls messages from the message queue to update warehouse stock states sequentially.
* ECommerce.UI: An interactive, single-page client administration panel built with Blazor WebAssembly.

------------------------------
## ⚡ The Concurrency Resolution Flow
To completely eliminate database multi-user lockouts and "double-selling" race conditions under heavy load, the write pathway is fully decoupled from the database transaction stream using an asynchronous event-driven pipeline:

[Blazor UI Client] ─── (HTTP POST /checkout) ───► [Minimal API Engine]
                                                           │
                                                   (Instantly Publishes)
                                                           │
                                                           ▼
                                                [CloudAMQP Message Broker]
                                                (Strict FIFO Queue Order)
                                                           │
                                                   (MassTransit Consumer)
                                                           │
                                                           ▼
[Neon Serverless DB] ◄─── (Async Persistence) ─── [Inventory Worker Service]


   1. Non-Blocking Submission: When a user checks out an item, the API captures the request, instantly wraps it into an OrderSubmittedEvent envelope, and broadcasts it to a cloud broker queue.
   2. Immediate Response: The API returns a 202 Accepted response back to the client immediately (within 2 milliseconds), keeping web network threads completely unblocked.
   3. Sequential Execution: MassTransit coordinates the message consumption pipeline over CloudAMQP (RabbitMQ), feeding events sequentially to the isolated background worker process to update your cloud Neon PostgreSQL database safely.

------------------------------
## 🛠️ Modern Cloud-Native Stack

* Core Runtime Framework: .NET 9.0 (Standard Term Support)
* Web API Design: ASP.NET Core Minimal APIs with native .NET 9 OpenAPI metadata tracking
* Distributed Orchestration: .NET Aspire AppHost Engine
* Message Broker Abstraction: MassTransit with automated Dead Letter Queue (DLQ) protection logic
* Cloud Messaging Transport: CloudAMQP (Managed Remote RabbitMQ Clusters)
* Relational Database Mapping: Entity Framework Core tracking serverless cloud Neon PostgreSQL
* Client User Interface: Blazor WebAssembly (WASM) 100% C# Frontend Engine

------------------------------
## 📂 Project Directory Map

📁 ModernCommerceEngine
 ├── ⚙️ ModernEcommerceEngine.AppHost         # Central .NET Aspire Orchestration & Dependency Injector
 ├── 📊 ModernEcommerceEngine.ServiceDefaults # Global telemetry, logging configurations, and health checks
 ├── 🌐 ECommerce.API                         # Thin entry API layer mapping endpoints & publishing messages
 ├── 🧠 ECommerce.Application                 # CQRS split logic models, MediatR handlers, and integration events
 ├── 💎 ECommerce.Domain                      # Core DDD entities containing private setters and encapsulation rules
 ├── 🧱 ECommerce.Infrastructure              # Data persistence configurations and remote cloud data seeder scripts
 ├── 🔀 ECommerce.InventoryWorker             # Background worker service processing MassTransit queue packets
 └── 🎨 ECommerce.UI                          # Blazor WebAssembly single-page application storefront catalog

------------------------------
## 🚀 Local Installation & Getting Started
Because this architecture relies fully on .NET Aspire Distributed Orchestration and external free-tier cloud configurations, you can spin up the entire multi-service mesh locally without setting up local database software or Docker environments.
## 📋 Prerequisites

* .NET 9.0 SDK
* Visual Studio 2022 (v17.12+ with .NET Aspire tools active)

## ⚙️ Step 1: Clone and Review Environment Keys
Clone the repository to your local machine and open the solution file inside Visual Studio. Open appsettings.json inside the ModernEcommerceEngine.AppHost project layer and ensure your connection strings are populated:

{
  "ConnectionStrings": {
    "PostgreSQL": "Host=your-neon-endpoint.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=your_password;SSL Mode=Require;Trust Server Certificate=true",
    "messaging": "amqps://your-user:your-password@://cloudamqp.com"
  }
}

## 🏁 Step 2: Boot the System

   1. Set ModernEcommerceEngine.AppHost as your single Startup Project inside Visual Studio.
   2. Press F5 or click the Play button to execute.
   3. Your browser will automatically launch the interactive .NET Aspire Developer Dashboard.
   4. From the dashboard interface, you can track cross-project log output, evaluate OpenTelemetry span traces, and select the ecommerce-ui endpoint link to navigate directly to the interactive customer catalog interface page!


