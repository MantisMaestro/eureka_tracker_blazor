# Agents.md - Developer & Agent Guide for Eureka Minecraft Website

Welcome to the **Eureka Minecraft Website** project context file. This document is designed to help AI agents and human developers quickly understand the project architecture, features, configuration, data structures, and deployment model.

---

## 1. Project Overview
The Eureka Minecraft Website is a web dashboard for the **Eureka Minecraft Server**. It provides:
- A landing page with server status, a dynamic image carousel showcasing builds, and social media/donation callouts.
- A **live interactive map** integrated directly into the page (powered by Dynmap or BlueMap via a reverse proxy).
- A **Players Tracker** that monitors current online players (with skin avatars) and leaderboard play times over various intervals (Daily, Weekly, Monthly, and Map Iterations).
- A **Portals Coordinate Converter** to help players align their Nether and Overworld portals.

---

## 2. Technology Stack & Dependencies

The project is built as a multi-project .NET solution targeting **.NET 10.0** (C#).

### Main Projects
1. **Client** (`Client.csproj`): A Blazor Web App (Interactive Server render mode) containing the UI, layout, pages, and background tracking service.
2. **EurekaDb** (`EurekaDb.csproj`): An Entity Framework Core class library managing the database context, schema, and migrations.

### Key Nuget Dependencies
- **[MudBlazor](https://mudblazor.com/) (v9.0.0)**: Component library used for the layout, cards, buttons, theme switching (dark/light mode), and carousel.
- **[Yarp.ReverseProxy](https://microsoft.github.io/reverse-proxy/) (v2.3.0)**: Used to proxy traffic to the live server map.
- **MCStatus.NET (v1.0.0)**: A library for querying Minecraft server status and active player lists.
- **Microsoft.EntityFrameworkCore.Sqlite (v10.0.3)**: Relational database provider for storing player data locally in a SQLite database.

---

## 3. Architecture & File Structure

```
.
├── .github/workflows/          # CI/CD Workflows
│   └── build-publish.yml       # Build, test, and deploy to production VPS
├── Client/                     # Blazor Frontend & Services
│   ├── Components/
│   │   ├── Common/             # Shared UI components (PlayerCard, PatreonCard, PlayNowCard, etc.)
│   │   ├── Layout/             # MainLayout containing MudThemeProvider and global nav
│   │   └── Pages/              # Page views: Home, Players, Portals, Error
│   ├── Extensions/             # C# extension methods (e.g. DateTimeExtensions.cs)
│   ├── Models/                 # Client-specific request/response data objects
│   ├── Properties/
│   │   └── launchSettings.json # Ports and environment variables for local testing
│   ├── Services/
│   │   ├── Data Service/       # Business logic layer for retrieving & updating player records
│   │   └── PingService.cs      # Background worker polling Minecraft server state
│   ├── wwwroot/                # Static assets (images, stylesheets, custom JS modules)
│   ├── appsettings.json        # Main configuration file
│   └── Program.cs              # Application startup, DI services, middleware, and auto-migration logic
├── EurekaDb/                   # EF Core Data Access Project
│   ├── Context/
│   │   └── EurekaContext.cs    # EF Core DB context configuration
│   ├── Migrations/             # SQLite migrations
│   └── Models/                 # Database tables entities (Player, PlayerSession)
└── EurekaTracker2.slnx         # Solution description file (modern Visual Studio XML format)
```

---

## 4. Key Features & Page Breakdown

### 🏠 Home Page (`Client/Components/Pages/Home.razor`)
- Displays server brand with carrot theme.
- Integrates social cards and the `PlayNowCard`.
- Houses a dynamic **image carousel** that automatically adjusts its height based on the loaded image size (assisted by JS module `Home.razor.js`).
- embeds an `<iframe>` pointing to `/map/` that renders the Minecraft map.

### 👥 Players Tracker (`Client/Components/Pages/Players.razor`)
- Retrieves a list of currently online players from the database.
- Displays leaderboards showing players who have played the most today, this week, this month, or on the current map iteration.
- Leverages the [Cravatar API](https://cravatar.eu/) inside `PlayerCard.razor` to load 3D skin heads using Minecraft player names: `https://cravatar.eu/helmavatar/{PlayerName}/64.png`.

### 🌌 Portals Calculator (`Client/Components/Pages/Portals.razor`)
- A helper page designed to convert coordinates between the Overworld and the Nether.
- *Calculation logic*: 1 Nether block = 8 Overworld blocks.
- Alerts players that the portal network on the server uses a standardized height of **Y = 110**.

### ⚙️ Reverse Proxy (`Client/Program.cs` & `appsettings.json`)
- Leverages YARP Reverse Proxy.
- Maps traffic on `/map/{**catch-all}` to the server hosting the BlueMap/Dynmap web interface (defaulting to port `25575`).

---

## 5. Database Schema

The SQLite database holds two tables defined in `EurekaDb/Context/EurekaContext.cs`:

### 1. `players` Table
Stores basic details and total cumulative play time for players who have logged onto the server.
*   **Id** (`string`, PK): Minecraft player UUID.
*   **Name** (`string`): Player username.
*   **LastOnline** (`DateTime?`): Timestamp of when the player was last seen online.
*   **TotalPlayTime** (`int?`): Total play time tracked in seconds.

### 2. `player_sessions` Table
Stores play times grouped by day for each player, allowing historical graphing and interval leaderboards.
*   **PlayerId** (`string`, PK/FK): Minecraft player UUID.
*   **Date** (`DateOnly`, PK): The specific calendar date.
*   **TimePlayedInSession** (`int?`): Play time tracked on that date in seconds.

*Relationship*: One Player has many PlayerSessions. Cascade delete is disabled (`ClientSetNull`).

---

## 6. Background Tracking Service (`Client/Services/PingService.cs`)

`PingService` is a `BackgroundService` that executes immediately when the app starts:
1. It runs a loop using a `PeriodicTimer` triggering every **60 seconds**.
2. On each tick, it loads server credentials (`Server:IP` and `Server:Port`) from settings.
3. It calls `ServerListClient.GetStatusAsync` (from `MCStatus.NET`) to poll the Minecraft server.
4. It extracts the current player list (`status.Players.Sample`).
5. Invokes `IDataService.UpdateLedger()` to add `60 seconds` to the active players' total play time and daily session records.

---

## 7. Configuration Keys (`appsettings.json`)

The following key sections in `appsettings.json` control app behavior:
*   `ConnectionStrings:DefaultConnection`: Connection string to the SQLite database (e.g. `Data Source=/srv/db/eureka` on VPS, `Data Source=local.db` in development).
*   `Server:IP`: The IP address of the Minecraft server used for background pinging.
*   `Server:Port`: The port of the Minecraft server used for background pinging.
*   `Server:CurrentMap` & `Server:NextMap`: Map iteration versions (used to toggle map-specific leaderboards).
*   `Server:CurrentMapStartDate` & `Server:NextMapStartDate`: Timestamps that determine when map-specific playtime starts and when the site automatically switches to displaying the next map iteration.
*   `ReverseProxy`: YARP routing rules mapping `/map/{**catch-all}` to the target map server address.

---

## 8. Resolved Issues & Areas of Improvement

During initial review, a few bugs were identified and successfully resolved:

1.  **Portals Page Math Bug** (`Client/Components/Pages/Portals.razor`): [RESOLVED]
    *   **Description**: The function `ConvertToOverworld()` (converting Nether coordinates to Overworld coordinates) incorrectly divided by 8 instead of multiplying.
    *   **Fix**: Modified `ConvertToOverworld()` to multiply the Nether coordinates by 8:
        ```csharp
        private void ConvertToOverworld()
        {
            OverworldX = Math.Floor(NetherX * 8);
            OverworldZ = Math.Floor(NetherZ * 8);
        }
        ```
2.  **PlayNowCard Hardcoding** (`Client/Components/Common/PlayNowCard.razor`): [RESOLVED]
    *   **Description**: The component previously referenced `Configuration["Server:IpAddress"]` which was empty, falling back to a hardcoded string `"173.240.152.72:9000"`.
    *   **Fix**: Updated the component to dynamically construct the IP from `Server:IP` and `Server:Port` and store it in `_configuredIpAddress`. The clipboard copy action now correctly resets to the dynamic configuration address rather than a hardcoded string.
3.  **Reverse Proxy Port Bindings**:
    *   YARP is configured to direct map requests to `http://173.240.152.72:25575` but the URL is hardcoded in the YARP configuration section in `appsettings.json` rather than referencing the `Server:IP` section.

---

## 9. CI/CD & Deployment (`.github/workflows/build-publish.yml`)

The application deploys to a VPS running Ubuntu on push to the `master` branch:
1.  **Build Phase**:
    *   Restores and builds the .NET application.
    *   Publishes the release build: `dotnet publish Client/Client.csproj -c Release`.
    *   Uploads the artifacts from `Client/bin/Release/net10.0/publish/`.
2.  **Deploy Phase**:
    *   Connects to VPS via SSH.
    *   Stops the server systemd service: `sudo systemctl stop EurekaTracker.service`.
    *   Uses `rsync` to synchronize files to `/var/www/eureka-website/`.
    *   Restarts the systemd service: `sudo systemctl start EurekaTracker.service`.

---

## 10. Development Instructions

### Running Locally
To launch the application locally, use the command line:
```bash
dotnet run --project Client/Client.csproj
```
This runs the application on the local port defined in `Client/Properties/launchSettings.json` (typically `http://localhost:5029` or `https://localhost:7224`). The database will default to `local.db` in the root of the `Client` project.

### Database Migrations
To add a new EF Core database migration:
```bash
dotnet ef migrations add <MigrationName> --project EurekaDb/EurekaDb.csproj --startup-project Client/Client.csproj
```
To update the local database:
```bash
dotnet ef database update --project EurekaDb/EurekaDb.csproj --startup-project Client/Client.csproj
```
*Note*: `Program.cs` automatically checks and applies pending database migrations on startup. It performs a database backup (`.bak` file) before running migration tasks to prevent accidental database corruption.
