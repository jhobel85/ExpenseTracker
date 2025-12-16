# Expense Tracker API

A RESTful API for tracking personal expenses built with ASP.NET Core and AWS RDS MS SQL.

## Features

- ✅ Create, read, and delete expenses
- ✅ Categorize expenses (Food, Transport, Bills, etc.)
- ✅ Filter expenses by date range
- ✅ Monthly expense summary by category
- ✅ Connects to AWS RDS MS SQL database

## Project Structure

```
ExpenseTracker/
├── Database/
│   └── schema.sql              # Database schema
├── src/
│   └── ExpenseTrackerAPI/      # ASP.NET Core Web API
│       ├── Models/             # Data models
│       ├── Services/           # Database service
│       ├── *Controller.cs      # API controllers
│       └── Program.cs
└── README.md
```

## Setup Instructions

### Step 1: Database Setup

1. Connect to your AWS RDS MS SQL database
2. Run the SQL script: `Database/schema.sql`
3. This will create:
   - `Categories` table with default categories
   - `Expenses` table
   - `vw_MonthlySummary` view

### Step 2: Configure Connection String

Edit `src/ExpenseTrackerAPI/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_RDS_ENDPOINT;Database=YOUR_DATABASE_NAME;User Id=YOUR_USERNAME;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
  }
}
```

Replace:
- `YOUR_RDS_ENDPOINT` - Your AWS RDS endpoint (e.g., `mydb.xxxxx.us-east-1.rds.amazonaws.com`)
- `YOUR_DATABASE_NAME` - Database name
- `YOUR_USERNAME` - Database username
- `YOUR_PASSWORD` - Database password

### Step 3: Run the API

```bash
cd src/ExpenseTrackerAPI
dotnet run
```

The API will start at: `https://localhost:5001` or `http://localhost:5000`

Swagger UI: `https://localhost:5001/swagger`

## API Endpoints

### Categories

- `GET /api/categories` - Get all categories

### Expenses

- `GET /api/expenses` - Get all expenses
  - Query params: `?startDate=2024-01-01&endDate=2024-12-31`
- `GET /api/expenses/{id}` - Get expense by ID
- `POST /api/expenses` - Create new expense
- `DELETE /api/expenses/{id}` - Delete expense
- `GET /api/expenses/summary/{year}/{month}` - Get monthly summary

### Example: Create Expense

```bash
POST /api/expenses
Content-Type: application/json

{
  "amount": 45.50,
  "categoryId": 1,
  "description": "Lunch at restaurant",
  "expenseDate": "2024-12-16"
}
```

## Testing with Swagger

1. Run the API: `dotnet run`
2. Open browser: `https://localhost:5001/swagger`
3. Test all endpoints interactively

## Next Steps

- [ ] Add authentication
- [ ] Create web UI
- [ ] Deploy to AWS (Elastic Beanstalk or App Runner)
- [ ] Add budget tracking
- [ ] Export to CSV/Excel

## Technologies

- ASP.NET Core 9.0
- Microsoft.Data.SqlClient
- AWS RDS MS SQL Server
- Swagger/OpenAPI
