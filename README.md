<div align="center">
  <img src="logo.png" alt="ADCCS Logo" width="150" />

  # 🛡️ Air Defence Command and Control System (ADCCS)
  
  **Advanced airspace monitoring, target detection, and defensive operations orchestration platform.**

  [![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
  [![ASP.NET Core MVC](https://img.shields.io/badge/ASP.NET_Core-MVC-blue?style=flat-square)](https://docs.microsoft.com/en-us/aspnet/core/)
  [![SQLite](https://img.shields.io/badge/SQLite-Database-003B57?style=flat-square&logo=sqlite)](https://sqlite.org/index.html)
  [![License](https://img.shields.io/badge/License-MIT-green.svg?style=flat-square)](LICENSE)

  <p align="center">
    <a href="#overview">Overview</a> •
    <a href="#key-features">Key Features</a> •
    <a href="#technology-stack">Technology Stack</a> •
    <a href="#system-architecture">Architecture</a> •
    <a href="#getting-started">Getting Started</a> •
    <a href="#documentation">Documentation</a>
  </p>
</div>

---

## 📖 Overview

The **Air Defence Command and Control System (ADCCS)** is a comprehensive, enterprise-grade web application engineered to simulate and manage critical aspects of modern air defense. Built on the robust ASP.NET Core MVC framework, ADCCS delivers a highly responsive, real-time command center interface. 

It provides operators with situational awareness, defensive asset management capabilities, and automated tracking systems essential for rapid decision-making in high-stakes environments.

---

## ✨ Key Features

- **📡 Real-Time Radar Visualization**
  Interactive, highly dynamic radar interface for tracking both stationary and moving targets. Utilizes **SignalR** to push immediate telemetry and status updates to all connected clients without requiring page reloads.

- **⚔️ Defensive Asset Management**
  Maintain and oversee a comprehensive registry of defensive capabilities. Organize and deploy assets across various categories:
  - 🛩️ Interceptor Aircraft
  - 🚀 Surface-to-Air / Air-to-Air Missiles (SAMs/AAMs)
  - 🚁 Unmanned Aerial Vehicles (UAVs / Drones)

- **🎯 Action Workflows & Orchestration**
  Seamlessly issue operational commands (e.g., *Intercept*, *Destroy*, *Observe*). The system meticulously tracks action workflows through states from initialization ("In Progress") to final resolution ("Completed").

- **⚙️ Automated Data Integrity via Database Triggers**
  Employs advanced SQLite database triggers (`AirDefence.db.sql`) to guarantee business logic enforcement at the data tier. Examples include:
  - Instantly marking a target as inactive upon the successful completion of a defensive action.
  - Auto-generating detailed mission logs the moment a new target is detected or an action is issued.

- **🎵 Persistent Media & Audio Engine**
  Features a continuous background audio engine that seamlessly persists playback state and timestamps across page navigations and reloads using synchronized session state, creating an immersive command center experience.

- **📋 Comprehensive Mission Logging**
  Extensive, immutable tracking of critical events, ensuring an auditable trail of new target detections, defensive actions taken, and operational state changes.

---

## 💻 Technology Stack

ADCCS leverages a modern, reliable technology stack to ensure performance, security, and real-time capabilities:

| Category | Technology |
| :--- | :--- |
| **Backend Framework** | .NET 10 (ASP.NET Core MVC) |
| **Database** | SQLite (via `AirDefence.db`) |
| **ORM** | Entity Framework Core |
| **Real-Time Engine** | SignalR |
| **Frontend** | Razor Pages, HTML5, Vanilla CSS3, JavaScript |
| **Authentication** | BCrypt.Net-Next |

---

## 🏗️ System Architecture

The application is strictly designed around the **Model-View-Controller (MVC)** pattern, ensuring clear separation of concerns and maintainability:

1. **Models:** Define the core domain entities (`Targets`, `Defensive Actions`, `Assets`, `Mission Logs`) and structure the Entity Framework Core database context.
2. **Views:** Premium Razor templates enriched with modern CSS3 animations, glassmorphism, and dynamic visual cues to provide an intuitive and reactive user interface.
3. **Controllers:** Manage incoming HTTP requests, orchestrate business logic workflows, and handle interactions with the data context.
4. **Database Tier:** Critical data consistency rules and automated logging are centralized directly within the SQLite schema via triggers, ensuring absolute data integrity independently of the application layer.

---

## 🚀 Getting Started

Follow these instructions to set up the project locally for development and testing.

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or higher.
- A modern web browser (Chrome, Firefox, Edge, Safari).

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
   *💡 Note: Upon the first startup, the application will automatically initialize, build the schema, and seed the SQLite database (`AirDefence.db`) if it is not already present.*

4. **Access the application:**
   Open your preferred web browser and navigate to the local URL provided in the console output (typically `http://localhost:5000` or `https://localhost:5001`).

---

## 📚 Project Documentation

Detailed project documentation, including in-depth architectural design, system capability analysis, and comprehensive workflow definitions, is available within the repository:

📄 **[ADCCS_Project_Report.docx](./ADCCS_Project_Report.docx)**

---

## 📄 License

This project is licensed under the **MIT License**. See the `LICENSE` file for full details.

---
<div align="center">
  <i>Developed for Advanced Agentic Coding Demonstration</i>
</div>
