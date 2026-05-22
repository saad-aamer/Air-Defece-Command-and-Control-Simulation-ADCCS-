# ADCCS (Air Defence Command and Control System)

![ADCCS Logo](logo.png)

## Overview

The **Air Defence Command and Control System (ADCCS)** is a comprehensive web-based application designed to simulate the monitoring of airspace, detection of aerial targets, and the orchestration of defensive operations. Built using ASP.NET Core MVC, ADCCS offers a dynamic, real-time command center interface.

## Key Features

- **Real-Time Radar Display:** Interactive radar visualization for tracking stationary and dynamic targets, utilizing SignalR to provide immediate updates without page reloads.
- **Defensive Asset Management:** Maintain and oversee an extensive registry of defensive assets, categorized into Aircraft, Missiles (SAMs/AAMs), and Drones/UAVs.
- **Action Workflows:** Seamlessly issue and monitor defensive commands (e.g., Intercept, Destroy). The system tracks workflows from "In Progress" to "Completed."
- **Automated Data Integrity:** Employs advanced SQLite database triggers to automate system state transitions. For example, marking a target as inactive instantly when a defensive action is completed, and automatically generating mission logs upon target detection or action issuance.
- **Persistent Media Engine:** Continuous background audio playback that seamlessly persists across different pages and navigation using session state synchronization.
- **Mission Logging:** Extensive tracking of events such as new target detections, action issuances, and overall operational logs.

## Technology Stack

- **Backend Framework:** .NET 10 (ASP.NET Core MVC)
- **Database:** SQLite
- **ORM:** Entity Framework Core
- **Real-Time Communication:** SignalR
- **Frontend:** Razor Pages, HTML5, Vanilla CSS3, JavaScript
- **Authentication:** BCrypt.Net-Next

## System Architecture

The application adheres to a strict Model-View-Controller (MVC) architectural pattern:

- **Models:** Define the data structures (Targets, Defensive Actions, Assets, Mission Logs) and Entity Framework data context.
- **Views:** Razor templates constructed with a premium, dynamic user interface utilizing modern CSS for animations and visual effects.
- **Controllers:** Handle HTTP requests, orchestrate application logic, and interact with the database context.
- **Database Triggers:** Critical data consistency rules and automated logging are centralized within the SQLite schema (`AirDefence.db.sql`), ensuring absolute data integrity even outside the application context.

## Getting Started

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later.

### Installation & Execution

1. **Clone the repository:**
   ```bash
   git clone <your-repo-url>
   ```

2. **Navigate to the project directory:**
   ```bash
   cd "ADCCS Web"
   ```

3. **Run the application:**
   ```bash
   dotnet run
   ```
   *Note: The application automatically initializes, creates, and seeds the SQLite database (`AirDefence.db`) upon first startup if it does not already exist.*

4. **Access the application:**
   Open your web browser and navigate to the local URL provided in the console output (typically `https://localhost:5001` or `http://localhost:5000`).

## Project Documentation

Detailed project documentation, including architectural design, system capabilities, and workflow definitions, is available within the `ADCCS_Project_Report.docx` file included in the repository.

## License

This project is licensed under the MIT License - see the LICENSE file for details.
