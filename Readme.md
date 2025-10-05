# Westmarchestool

A Pathfinder 2e Society West Marches campaign management web application.

## Features (Planned)
- User authentication and authorization (Admin, GM, Player roles)
- Character creation and management
- Lore wiki with knowledge graph and wiki-links
- Shop system with gold and reputation currencies
- Dynamic hex-based exploration map

## Tech Stack
- **Backend:** ASP.NET Core 9.0 Web API
- **Frontend:** Vanilla JavaScript SPA with Web Components
- **Database:** SQLite (development), PostgreSQL (production)
- **Authentication:** JWT tokens with ASP.NET Core Identity

## Project Structure
- `Westmarchestool.API/` - Backend Web API
- `Westmarchestool.Web/` - Frontend static files

## Getting Started

### Prerequisites
- .NET 9.0 SDK
- Visual Studio 2022

### Running Locally
1. Clone the repository
2. Open `Westmarchestool.sln` in Visual Studio
3. Both projects should be set as startup projects
4. Press F5 to run
5. API runs on: https://localhost:7157
6. Web runs on: https://localhost:7032