# ShopZone — Full Stack E-Commerce App

**.NET 8 Backend** (SQL Server, Service Layer, Repository Pattern) + **Angular 17 Frontend**

---

## 🏗 Backend Architecture

```
backend/
├── Controllers/          ← Thin — routing + HTTP only, zero business logic
│   ├── AuthController.cs
│   ├── ProductsController.cs
│   ├── CartController.cs
│   └── OrdersController.cs
│
├── Interfaces/           ← Service contracts
│   ├── IAuthService.cs
│   ├── IProductService.cs
│   ├── ICartService.cs
│   └── IOrderService.cs
│
├── Services/
│   ├── JwtService.cs     ← Token generation
│   └── Impl/             ← Business logic lives here
│       ├── AuthService.cs
│       ├── ProductService.cs
│       ├── CartService.cs
│       └── OrderService.cs
│
├── Models/               ← EF Core entities
├── DTOs/                 ← Request/response shapes
├── Data/                 ← AppDbContext (SQL Server)
└── Migrations/           ← EF Core migrations (auto-applied on startup)
```

### Layers
| Layer | Responsibility |
|---|---|
| **Controller** | Parse HTTP request → call service → return HTTP response |
| **Interface** | Service contract (for testability / DI) |
| **Service** | All business logic, DB access via EF Core |
| **Model** | EF Core entity mapped to SQL Server table |
| **DTO** | Serialisation shapes (never expose models directly) |

---

## 🚀 Backend Setup

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server (local or Docker)

### SQL Server via Docker (quickest)
```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourPass123!" \
  -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
```
Then update `appsettings.json`:
```json
"DefaultConnection": "Server=localhost;Database=ShopZoneDb;User Id=sa;Password=YourPass123!;TrustServerCertificate=True;"
```

### Run
```bash
cd backend
dotnet restore
dotnet run
# Migrations run automatically on startup → DB + seed data created
```
API: `http://localhost:5000`

### Stripe Keys
Edit `appsettings.json`:
```json
"Stripe": {
  "SecretKey": "sk_test_YOUR_KEY",
  "PublishableKey": "pk_test_YOUR_KEY"
}
```

### Default Admin Account
- Email: `admin@shop.com`
- Password: `admin123`

---

## 🎨 Frontend Setup

### Prerequisites
- Node.js 18+
- `npm install -g @angular/cli`

### Run
```bash
cd frontend
npm install
ng serve
```
App: `http://localhost:4200`

---

## 📋 API Endpoints

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | /api/auth/register | Public | Register user |
| POST | /api/auth/login | Public | Login |
| GET | /api/products | Public | List products (filter/search) |
| GET | /api/products/:id | Public | Get product |
| GET | /api/products/categories | Public | Get categories |
| GET | /api/products/admin/all | Admin | All products incl. inactive |
| POST | /api/products | Admin | Create product |
| PUT | /api/products/:id | Admin | Update product |
| DELETE | /api/products/:id | Admin | Soft-delete product |
| GET | /api/cart | User | Get cart |
| POST | /api/cart | User | Add to cart |
| PUT | /api/cart/:id | User | Update quantity |
| DELETE | /api/cart/:id | User | Remove item |
| DELETE | /api/cart | User | Clear cart |
| POST | /api/orders/create-payment-intent | User | Init Stripe payment |
| POST | /api/orders/confirm | User | Confirm order |
| GET | /api/orders | User | My orders |
| GET | /api/orders/:id | User | Single order |
| GET | /api/orders/admin/all | Admin | All orders |

---

## 💳 Stripe Test Cards
| Card | Result |
|---|---|
| `4242 4242 4242 4242` | Success |
| `4000 0000 0000 9995` | Declined |

Any future expiry + any 3-digit CVC.

---

## 🛠 Tech Stack

**Backend:** ASP.NET Core 8 · EF Core 8 · SQL Server · JWT · BCrypt · Stripe.net

**Frontend:** Angular 17 (standalone) · RxJS · Reactive Forms · SCSS · Lazy routing
