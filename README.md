<h1 align="center">🏥 Clinic Management System</h1>

<p align="center">
  <strong>A modern, secure, and fully-featured Clinic Management System built with ASP.NET Core MVC 10.</strong><br>
  Designed to streamline medical facility operations with robust Role-Based Access Control (RBAC), clean architecture, and optimized data access.
</p>

<p align="center">
  <a href="http://clinicmanagementsystem.somee.com" target="_blank">
    <img src="https://img.shields.io/badge/🚀_Live_Demo-Access_Portal-2ea44f?style=for-the-badge" alt="Live Demo" />
  </a>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=dotnet" alt=".NET 10" />
  <img src="https://img.shields.io/badge/ASP.NET_Core_MVC-10.0-blue?style=flat-square" alt="ASP.NET Core MVC" />
  <img src="https://img.shields.io/badge/SQLite-07405E?style=flat-square&logo=sqlite&logoColor=white" alt="SQLite" />
  <img src="https://img.shields.io/badge/EF_Core-10.0-33A25A?style=flat-square" alt="Entity Framework Core" />
  <img src="https://img.shields.io/badge/Bootstrap-5.3-7952B3?style=flat-square&logo=bootstrap&logoColor=white" alt="Bootstrap" />
</p>

---

## 📌 Overview

The **Clinic Management System** is an enterprise-grade web application developed to digitalize healthcare workflows. It provides distinct, secure portals for Administrators, Doctors, Receptionists, and Patients. By implementing **Separation of Concerns (SoC)** and a **Service-Oriented Architecture (SOA)**, the system ensures high maintainability, testability, and scalability.

## 🔗 Live Demo
* **URL:** [http://clinicmanagementsystem.somee.com](http://clinicmanagementsystem.somee.com)

**Demo Accounts:**
| Role | Email | Password |
| :--- | :--- | :--- |
| **Admin** | `admin@clinic.com` | `Admin123!` |
| **Doctor** | `Dsara@clinic.com` | `Doctor123!` |
| **Receptionist** | `ReMohamed@clinic.com` | `Receptionist123!` |
| **Patient** | `Phesham@gmail.com` | `1234567890Ha#` |

> [!NOTE]
> **Important:** The live demo is hosted on a free tier which does not provide an SSL certificate by default. Please ensure you access the site using **`http://`** and not `https://`. You may safely ignore any "Not Secure" browser warnings during testing.

---

## ✨ Core Features

### 🛡️ 1. Advanced Role-Based Access Control (RBAC)
The system leverages ASP.NET Core Identity to enforce strict authorization policies across four distinct roles:
- **👑 Administrator:** Manages the overarching system, including provisioning Doctor and Receptionist accounts securely, managing medical specialties, and monitoring high-level clinic metrics.
- **👨‍⚕️ Doctor:** Accesses a personalized dashboard to view daily schedules, manage assigned patient appointments, and update professional profiles.
- **👩‍💻 Receptionist:** Acts as the operational bridge; manages patient records, handles offline appointment bookings, and oversees appointment statuses (Confirm/Cancel).
- **🤒 Patient:** A dedicated self-service portal allowing patients to browse available doctors by specialty, book appointments based on real-time doctor availability, and track their medical history.

### 🏗️ 2. Architectural Best Practices
- **Service Layer (IUserService):** Identity management logic (creation, updating, deletion of users) is abstracted away from Controllers into dedicated injectable services, drastically reducing "Fat Controllers" and adhering to **SOLID** principles.
- **Dependency Injection:** Fully utilizes ASP.NET Core's built-in IoC container for decoupling services, DbContexts, and Identity managers.

### 🔒 3. Security & Data Integrity
- **Dynamic Secure Provisioning:** The system automatically generates cryptographically secure, temporary passwords when an Admin creates a new staff account, eliminating the anti-pattern of hardcoded initial passwords.
- **Entity Validation:** Comprehensive server-side and client-side validation using Data Annotations (e.g., strict `[StringLength]` and regex boundaries) to prevent SQL Injection and buffer overflow vulnerabilities.
- **CSRF Protection:** Native Anti-Forgery Tokens are enforced on all state-mutating requests.

### ⚡ 4. Performance Optimization
- **Optimized EF Core Queries:** High-traffic dashboards utilize database-level aggregations (e.g., `CountAsync()`) instead of loading entities into memory (`ToListAsync()`), minimizing RAM consumption and preventing `OutOfMemoryExceptions` at scale.

---

## 🛠️ Technology Stack

| Component | Technology |
| :--- | :--- |
| **Backend Framework** | .NET 10.0, ASP.NET Core MVC |
| **Data Access** | Entity Framework Core 10.0 (Code-First) |
| **Database** | SQLite (Lightweight, file-based persistence) |
| **Authentication** | ASP.NET Core Identity (Cookie-based auth) |
| **Frontend** | HTML5, CSS3, Razor Views (`.cshtml`), Bootstrap 5 |

---

## 🚀 Getting Started (Local Development)

### Prerequisites
*   [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or higher.
*   [Visual Studio 2022](https://visualstudio.microsoft.com/) / JetBrains Rider / VS Code.

### Installation & Setup

1. **Clone the repository:**
   ```bash
   git clone https://github.com/HazemAshrafEmamm/ClinicManagementSystem.git
   cd ClinicManagementSystem
   ```

2. **Restore NuGet Packages:**
   ```bash
   dotnet restore
   ```

3. **Database Migration & Initialization:**
   The application uses SQLite and is configured to seed the initial Admin account and Roles automatically. To apply migrations and create `app.db`:
   ```bash
   dotnet ef database update
   ```

4. **Run the Application:**
   ```bash
   dotnet run
   ```
   *The application will be accessible at `https://localhost:xxxx`.*

---

## 📂 Project Architecture

```text
ClinicManagementSystem/
├── Controllers/         # Handles HTTP routing and orchestrates data flow
├── Models/              # Domain entities (Doctor, Patient, Appointment, Specialty)
├── ViewModels/          # DTOs specifically shaped for Razor Views
├── Views/               # UI Layer (Razor Templates organized by Controller)
├── Services/            # Business Logic Layer (e.g., UserService, IUserService)
├── Data/                # EF Core ApplicationDbContext & Database Seeder logic
├── wwwroot/             # Static assets (CSS, JS, Libs, Images)
├── Program.cs           # Application entry point, DI Container, and Middleware pipeline
└── appsettings.json     # Application configuration & connection strings
```

---

## 🤝 Contributing

Contributions, issues, and feature requests are welcome! 
If you'd like to contribute, please fork the repository and use a feature branch. Pull requests are warmly welcome.

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---
<p align="center">
  <i>Developed with ❤️ by <a href="https://github.com/HazemAshrafEmamm">Hazem Ashraf</a></i>
</p>
