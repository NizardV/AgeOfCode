# AgeOfCode

> Application client-serveur temps réel avec interface terminal interactive / Real-time client-server app with interactive terminal UI

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-512BD4?style=flat&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/apps/aspnet)
[![SignalR](https://img.shields.io/badge/SignalR-WebSockets-512BD4?style=flat&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/apps/aspnet/signalr)
[![Terminal.Gui](https://img.shields.io/badge/Terminal.Gui-TUI-555555?style=flat)](https://github.com/gui-cs/Terminal.Gui)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?style=flat&logo=postgresql&logoColor=white)](https://www.postgresql.org)
[![Entity Framework Core](https://img.shields.io/badge/EF%20Core-8.0-512BD4?style=flat&logo=dotnet&logoColor=white)](https://learn.microsoft.com/ef/core)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?style=flat&logo=docker&logoColor=white)](https://www.docker.com)

## 🇫🇷 Français | 🇬🇧 English

[Voir en français](#-présentation) | [View in English](#-overview)

---

## 🇫🇷 Présentation

AgeOfCode est une application client-serveur temps réel développée avec .NET 8. Le serveur expose une WebAPI ASP.NET Core couplée à SignalR pour la communication par WebSockets. Le client est une application en ligne de commande avec interface terminal interactive (TUI) construite avec Terminal.Gui. Projet P1 DIIAGE basé sur un repo fourni par l'école.

## Stack technique

| Composant | Technologies |
|-----------|-------------|
| Serveur | C#, ASP.NET Core, SignalR, Entity Framework Core |
| Client | C#, Terminal.Gui (TUI) |
| Base de données | PostgreSQL |
| Infrastructure | Docker, Docker Compose (PostgreSQL + PgAdmin) |
| Tests | xUnit — tests unitaires (Server.Tests/) |

## Architecture

```
AgeOfCode/
├── Client/         # Application CLI/TUI (Terminal.Gui)
│   └── Connexion à l'API et au hub SignalR
├── Server/         # WebAPI ASP.NET Core + hub SignalR
│   └── Contrôleurs REST, hub temps réel, EF Core + PostgreSQL
└── Server.Tests/   # Tests unitaires du serveur
```

## Fonctionnalités principales

- Interface terminal interactive (TUI) avec Terminal.Gui
- Communication temps réel via WebSockets (SignalR)
- Base de données PostgreSQL avec migrations EF Core
- Administration de la base via PgAdmin (Docker)
- Tests unitaires du serveur

## Lancer en local

### Prérequis

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/get-started)

### 1. Base de données (Docker)

```bash
docker-compose up -d
```

PostgreSQL accessible sur `localhost:5432` · PgAdmin sur `http://localhost:8080`
(identifiants : `wss@wss.com` / `WSS`)

### 2. Appliquer les migrations EF Core

```bash
cd Server
dotnet ef database update
```

### 3. Lancer le serveur

```bash
cd Server
dotnet run --launch-profile https
```

API disponible sur `https://localhost:7032`.

### 4. Lancer le client TUI

```bash
cd Client
dotnet run
```

### Lancer les tests

```bash
cd Server.Tests
dotnet test
```

> Documentation complète de setup : [README-SETUP.md](./README-SETUP.md)

## Équipe

Projet P1 DIIAGE — 3 contributeurs

| Rôle | Nom |
|------|-----|
| Développeur | [À COMPLÉTER] |
| Développeur | [À COMPLÉTER] |
| Développeur | [À COMPLÉTER] |

---

## 🇬🇧 Overview

AgeOfCode is a real-time client-server application built with .NET 8. The server exposes an ASP.NET Core WebAPI combined with SignalR for WebSocket communication. The client is a command-line application with an interactive terminal UI (TUI) built with Terminal.Gui. P1 DIIAGE project based on a school-provided repository.

## Tech stack

| Component | Technologies |
|-----------|-------------|
| Server | C#, ASP.NET Core, SignalR, Entity Framework Core |
| Client | C#, Terminal.Gui (TUI) |
| Database | PostgreSQL |
| Infrastructure | Docker, Docker Compose (PostgreSQL + PgAdmin) |
| Tests | xUnit — unit tests (Server.Tests/) |

## Architecture

```
AgeOfCode/
├── Client/         # CLI/TUI application (Terminal.Gui)
│   └── Connects to the API and SignalR hub
├── Server/         # ASP.NET Core WebAPI + SignalR hub
│   └── REST controllers, real-time hub, EF Core + PostgreSQL
└── Server.Tests/   # Server unit tests
```

## Key features

- Interactive terminal UI (TUI) with Terminal.Gui
- Real-time communication via WebSockets (SignalR)
- PostgreSQL database with EF Core migrations
- Database administration via PgAdmin (Docker)
- Server unit tests

## Run locally

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/get-started)

### 1. Database (Docker)

```bash
docker-compose up -d
```

PostgreSQL at `localhost:5432` · PgAdmin at `http://localhost:8080`
(credentials: `wss@wss.com` / `WSS`)

### 2. Apply EF Core migrations

```bash
cd Server
dotnet ef database update
```

### 3. Start the server

```bash
cd Server
dotnet run --launch-profile https
```

API available at `https://localhost:7032`.

### 4. Start the TUI client

```bash
cd Client
dotnet run
```

### Run tests

```bash
cd Server.Tests
dotnet test
```

> Full setup documentation: [README-SETUP.md](./README-SETUP.md)

## Team

P1 DIIAGE project — 3 contributors

| Role | Name |
|------|------|
| Developer | [TO COMPLETE] |
| Developer | [TO COMPLETE] |
| Developer | [TO COMPLETE] |
