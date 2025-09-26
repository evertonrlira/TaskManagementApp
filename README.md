# Task Management Application (aka: TaskPuma)

A full-stack task management application built with .NET Core and React, demonstrating clean architecture and modern development practices.

## Quick Start

**Prerequisites:** [.NET 9 SDK](https://dotnet.microsoft.com/download) and [Node.js (v18+)](https://nodejs.org/)

From the project root:

```sh
npm install
npm run start
```

This starts both the .NET Core API (port 5000) and React frontend (port 5173). Access the app at [http://localhost:5173](http://localhost:5173).

## Testing

**Backend Unit Tests:** Comprehensive integration tests using xUnit with WebApplicationFactory for real API testing.

**E2E Tests:** Full application testing using Playwright to verify user workflows.

## Technical Decisions

**Architecture:** Clean Architecture with CQRS pattern for maintainable, testable code.

**Database:** EF Core In-Memory database. Simple setup for demo purposes, though production would use PostgreSQL/SQL Server.

**Frontend:** React + TypeScript with Vite for fast development and Tailwind for responsive design.

**API:** RESTful design with proper HTTP methods, validation, and error handling.

## Trade-offs & Assumptions

- **In-Memory DB:** Fast development but no persistence between restarts
- **No Authentication:** Focused on core architecture rather than auth complexity
- **Multi-user Support:** Built-in but simplified
- **Task Limits:** Max lengths match Google Tasks for potential future integration

## Production Considerations

**Scalability:** Database migration to PostgreSQL or SQL Server, connection pooling, caching layer (Redis), and microservices when needed.

**Security:** JWT authentication, HTTPS, rate limiting, OAuth integration, and audit logging.

**Future Features:** Task categories, due dates, file attachments, bulk operations, and notifications.
