# ✈️ Flight Booking System V2

A production-grade RESTful API for airline booking operations built with **ASP.NET Core 8**, **C# 12**, **Entity Framework Core 8**, and **SQL Server 2022**. The system implements a strict **3-tier layered architecture** with JWT authentication, BCrypt password hashing, SMTP email notifications, GST fare calculation, and a comprehensive unit test suite.

---

## 📌 Features

- 🔐 **JWT Authentication** — HMAC-SHA256 signed tokens with role-based access control (Customer & Admin)
- 🔑 **BCrypt Password Hashing** — Secure credential storage via BCrypt.Net-Next
- 🛫 **Flight Search** — Filter by origin, destination, and date (only seats-available flights returned)
- 💰 **Fare Calculation** — Base fare + 18% GST breakdown via dedicated FareService
- 🎟️ **Flight Booking** — Unique reference generation, seat decrement, booking confirmation
- 🪑 **Online Check-In** — Auto seat assignment (e.g. `14C`), check-in reference, status update
- 📧 **SMTP Email Notifications** — Welcome, Booking Confirmation, Check-In Confirmation via Gmail
- 🛡️ **Admin Subsystem** — Full CRUD flight management with safe-delete (409 if bookings exist)
- ⚠️ **Global Exception Handling** — Centralized middleware mapping exceptions to HTTP status codes
- 📄 **Swagger UI** — Interactive API documentation with JWT Bearer authorization
- 🧪 **55 Unit Tests** — NUnit + Moq across 10 test files covering all controllers and services

---

## 🏗️ Architecture

```
Client (Swagger / Postman / Frontend)
        │
        ▼
┌─────────────────────────────────────────┐
│          Middleware Pipeline            │
│  GlobalExceptionHandler │ JWT Auth      │
└─────────────────────────────────────────┘
        │
        ▼
┌─────────────────────────────────────────┐
│       Presentation Layer (Controllers)  │
│  Auth │ Admin │ Flights │ Fare          │
│  Booking │ CheckIn                      │
└─────────────────────────────────────────┘
        │
        ▼
┌─────────────────────────────────────────┐
│       Service Layer (Business Logic)    │
│  Auth │ Admin │ Flight │ Fare           │
│  Booking │ CheckIn │ Email             │
└─────────────────────────────────────────┘
        │
        ▼
┌─────────────────────────────────────────┐
│   Data Access Layer (Repositories)      │
│  Auth │ User │ Flight │ Fare            │
│  Booking │ CheckIn │ AdminFlight        │
│  Delete │ UserRepo                      │
└─────────────────────────────────────────┘
        │
        ▼
┌─────────────────────────────────────────┐
│     SQL Server — FlightBookingDbV2      │
│  Customers │ Admins │ Flights           │
│  Bookings  │ CheckIns                  │
└─────────────────────────────────────────┘
```

---

## 🛠️ Tech Stack

| Component | Technology |
|---|---|
| Framework | ASP.NET Core 8 |
| Language | C# 12 |
| Database | SQL Server 2022 |
| ORM | Entity Framework Core 8 (Code First) |
| Authentication | JWT Bearer — HMAC-SHA256 |
| Password Security | BCrypt.Net-Next |
| Email | SMTP — System.Net.Mail (Gmail) |
| API Docs | Swagger / Swashbuckle |
| Unit Testing | NUnit 3 + Moq 4 |

---

## 📁 Project Structure

```
FlightBookingSystem/
│
├── Controllers/
│   ├── AuthController.cs
│   ├── AdminController.cs
│   ├── FlightsController.cs
│   ├── FareController.cs
│   ├── BookingController.cs
│   └── CheckInController.cs
│
├── Services/
│   ├── Interfaces/
│   │   ├── IAuthService.cs
│   │   ├── IAdminService.cs
│   │   ├── IFlightService.cs
│   │   ├── IFareService.cs
│   │   ├── IBookingService.cs
│   │   ├── ICheckInService.cs
│   │   └── IEmailService.cs
│   └── Implementations/
│       ├── AuthService.cs
│       ├── AdminService.cs
│       ├── FlightService.cs
│       ├── FareService.cs
│       ├── BookingService.cs
│       ├── CheckInService.cs
│       └── EmailService.cs
│
├── Repositories/
│   ├── Interfaces/
│   │   ├── IAuthRepository.cs
│   │   ├── IUserRepository.cs
│   │   ├── IFlightRepository.cs
│   │   ├── IFareRepository.cs
│   │   ├── IBookingRepository.cs
│   │   ├── ICheckInRepository.cs
│   │   ├── IAdminFlightRepository.cs
│   │   └── IDeleteRepository.cs
│   └── Implementations/
│       ├── AuthRepository.cs
│       ├── UserRepository.cs
│       ├── FlightRepository.cs
│       ├── FareRepository.cs
│       ├── BookingRepository.cs
│       ├── CheckInRepository.cs
│       ├── AdminFlightRepository.cs
│       └── DeleteRepository.cs
│
├── Models/
│   ├── Customer.cs
│   ├── Admin.cs
│   ├── Flight.cs
│   ├── Booking.cs
│   ├── CheckIn.cs
│   └── DTOs/
│       ├── Request/
│       └── Response/
│
├── Data/
│   └── FlightDbContext.cs
│
├── Middleware/
│   └── GlobalExceptionHandlerMiddleware.cs
│
├── Program.cs
├── appsettings.json
└── appsettings.Development.json

FlightBookingSystem.Tests/
├── BookingControllerTests.cs
├── CheckInControllerTests.cs
├── AuthControllerTests.cs
├── AdminControllerTests.cs
├── FlightControllerTests.cs
├── FareControllerTests.cs
├── BookingServiceTests.cs
├── AuthServiceTests.cs
├── CheckInServiceTests.cs
├── EmailServiceTests.cs
└── Helpers/
    └── DbSetMockHelper.cs
```

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server 2022](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or VS Code
- Gmail account with App Password enabled (for SMTP)

### 1. Clone the Repository

```bash
git clone https://github.com/your-username/FlightBookingSystem.git
cd FlightBookingSystem
```

### 2. Configure appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER\\MSSQLSERVER02;Database=FlightBookingDbV2;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key-min-32-characters",
    "Issuer": "FlightBookingSystem",
    "Audience": "FlightBookingUsers"
  },
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "your-email@gmail.com",
    "SenderPassword": "your-app-password",
    "SenderName": "FlightBooking System"
  }
}
```

### 3. Apply Migrations and Run

```bash
# Restore packages
dotnet restore

# Apply EF Core migrations (or let EnsureCreated handle it)
dotnet ef database update

# Run the application
dotnet run
```

### 4. Access Swagger UI

```
http://localhost:5000/swagger
```

Click **Authorize** in Swagger UI and enter your JWT token as `Bearer <token>` to test protected endpoints.

---

## 🔑 Default Admin Credentials

```
Username : admin
Password : admin123
```

> Admin account is seeded automatically on first startup via `Program.cs`.

---

## 📡 API Endpoints

### Auth
| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | `/api/auth/user/register` | Public | Register new customer |
| POST | `/api/auth/user/login` | Public | Login and get JWT token |

### Flights & Fare
| Method | Endpoint | Auth | Description |
|---|---|---|---|
| GET | `/api/flights/search` | Public | Search flights by origin, destination, date |
| GET | `/api/fare/get` | Public | Get fare by flight number (Base + 18% GST) |
| GET | `/api/fare/{flightId}` | Public | Get fare by flight ID |

### Booking
| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | `/api/bookings/book` | Customer JWT | Create a new booking |
| POST | `/api/bookings/search` | Public | Get booking by reference number |

### Check-In
| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | `/api/checkin/search` | Public | Search booking for check-in |
| POST | `/api/checkin/perform` | Public | Perform check-in and get seat assignment |
| GET | `/api/checkin/status/{ref}` | Public | Get check-in status by reference |

### Admin
| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | `/api/admin/login` | Public | Admin login — returns Role=Admin JWT |
| POST | `/api/admin/flights` | Admin JWT | Create new flight |
| PUT | `/api/admin/flights` | Admin JWT | Update existing flight |
| DELETE | `/api/admin/flights/{id}` | Admin JWT | Delete flight (409 if bookings exist) |
| PUT | `/api/admin/users` | Admin JWT | Update customer details |

---

## 🧪 Running Tests

```bash
cd FlightBookingSystem.Tests
dotnet test
```

Expected output:
```
Total tests: 55
Passed:      55
Failed:      0
```

---

## 🗄️ Database Schema

| Table | Key Columns |
|---|---|
| Customers | Id, Username, Email, Password (BCrypt) |
| Admins | Id, Username, Email, Password (BCrypt) |
| Flights | FlightId, FlightNumber, Origin, Destination, FlightDate, Fare, AvailableSeats |
| Bookings | BookingId, ReferenceNumber, FlightId (FK), CustomerId (FK), FirstName, LastName, Gender, BookingDate, BookingStatus, BaseFare, FinalFare, IsCheckedIn, SeatNumber, CheckInDate |
| CheckIns | CheckInId, BookingId (FK, Unique), SeatNumber, CheckInReference, CheckInStatus |

---

## 🔒 Security

- Passwords hashed with **BCrypt** — never stored in plain text
- JWT tokens signed with **HMAC-SHA256**
- Customer tokens expire in **1 hour**
- Admin tokens expire in **2 hours** with `Role=Admin` claim
- Admin endpoints protected with `[Authorize(Roles = "Admin")]`
- `GlobalExceptionHandlerMiddleware` prevents stack trace exposure
- Safe-delete on flights returns **409 Conflict** when bookings exist

---

## 📧 Email Triggers

| Event | Email Sent |
|---|---|
| User Registration | Welcome email with account details |
| Flight Booking | Booking confirmation with reference, flight, and fare |
| Online Check-In | Check-in confirmation with seat number and CI reference |

---

## 📊 Project Stats

- **15+** RESTful API endpoints
- **5** database tables
- **55** unit tests (NUnit + Moq)
- **3** SMTP email triggers
- **18%** GST applied to all fares
- **150** default seats per flight
- **50+** source files

---

## 🔮 Future Enhancements

- [ ] Refresh token implementation
- [ ] Rate limiting on auth endpoints
- [ ] Flight cancellation workflow
- [ ] Docker containerization
- [ ] Azure Key Vault for secrets management
- [ ] Pagination on flight search results
- [ ] Microservices evolution with message queue (RabbitMQ)

---

## 👨‍💻 Author

**Pratham V**
B.Tech Computer Science and Technology
Presidency University, Bengaluru

---

## 📄 License

This project is developed for academic and portfolio purposes.
