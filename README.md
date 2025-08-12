# MyCarSharingApp

A **Car Sharing Application** built with **ASP.NET Core 8**, **Entity Framework Core 9**, and **PostgreSQL**.  
Implements a clean architecture with separated layers:  
- **Domain** – entities and business rules  
- **Application** – services, DTOs, and interfaces  
- **Infrastructure** – database access (EF Core + PostgreSQL)  
- **API** – controllers, endpoints, authentication (JWT)  

---

## 🚀 Features
- **User Authentication** using JWT + ASP.NET Identity
- **Car Management**:
  - Add new cars
  - Get car by ID
  - Delete cars
- **Rental Management**:
  - Rent a car (only if available)
  - Set actual return date for rentals
  - Prevent double rentals
- **Transactions** for critical operations
- **HMAC** password hashing
- **PostgreSQL** database integration

---

## 📂 Project Structure
                    MyCarSharingApp/
                    │
                    ├── MyCarSharingApp.Domain/ # Entities (Car, Rental, User)
                    ├── MyCarSharingApp.Application/ # Services, Interfaces, DTOs
                    ├── MyCarSharingApp.Infrastructure/ # EF Core, PostgreSQL, Repositories
                    ├── MyCarSharingApp.API/ # Controllers, Startup
                    └── MyCarSharingApp.Tests/ # Unit tests (xUnit + Moq)

---

## ⚙️ Prerequisites
- **.NET 8 SDK** or higher  
- **PostgreSQL** (v14+)  
- Visual Studio 2022 or VS Code  

---

## 🔧 How to Run

1. **Clone the repository**

git clone https://github.com/loranazarenko/MyCarSharingApp.git
cd MyCarSharingApp
Configure the database

Open appsettings.json in MyCarSharingApp.API

Set your PostgreSQL connection string:


"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=CarSharingDB;Username=postgres;Password=yourpassword"
}

Apply migrations

cd MyCarSharingApp.API
dotnet ef database update
Run the application

dotnet run --project MyCarSharingApp.API
📜 API Endpoints
Cars
POST /api/cars – Add a new car

GET /api/cars/{id} – Get car by ID

DELETE /api/cars/{id} – Delete a car

Rentals
POST /api/rentals – Rent a car

PUT /api/rentals/{id}/return – Set actual return date

🧪 Running Tests
We use xUnit and Moq for unit testing.

Run tests:

dotnet test
Example Tests
CarServiceTests

AddNewCarAsync_ShouldAddCar_WhenDataIsValid

GetCarByIdAsync_ShouldReturnNull_WhenCarDoesNotExist

RentalServiceTests

RentCarAsync_ShouldSucceed_WhenCarIsAvailable

RentCarAsync_ShouldThrowException_WhenCarNotAvailable

🛠 Manual Testing in Swagger
After running the app (dotnet run), open:

https://localhost:5001/swagger
Test Scenarios:

Add a Car → Use POST /api/cars

Get Car by ID → Use GET /api/cars/{id}

Rent a Car → Use POST /api/rentals

Return Car → Use PUT /api/rentals/{id}/return

Error Testing:

Rent an already rented car

Delete non-existent car

Return already returned rental

📄 License
MIT License
