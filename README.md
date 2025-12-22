# Expense Tracker API

A RESTful API for tracking personal expenses built with ASP.NET Core and MSSQLLocalDB (AWS RDS MS SQL optional).

## Features

- ✅ Create, read, and delete expenses
- ✅ Categorize expenses (Food, Transport, Bills, etc.)
- ✅ Filter expenses by date range
- ✅ Monthly expense summary by category
- ✅ Connects to MSSQLLocalDB database (AWS RDS MS SQL optional)

## Setup Instructions

### Step 1: Database Setup

1. For MSSQLLocalDB (default): The database and schema will be created automatically when the API starts.
2. For AWS RDS MS SQL: Connect to your AWS RDS MS SQL database and run the SQL script: `Database/schema.sql` (optional, as it's executed automatically)

### Step 2: Configure Connection String

Edit `src/ExpenseTrackerAPI/appsettings.json`:

For MSSQLLocalDB (default):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ExpenseTracker;Trusted_Connection=True;"
  }
}
```

For AWS RDS MS SQL:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_RDS_ENDPOINT;Database=YOUR_DATABASE_NAME;User Id=YOUR_USERNAME;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
  }
}
```

Password is stored using local dotnet secret store. Use variable ${DB_PASSWORD}

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

The API will start at: `https://localhost:5001`

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

## Web UI

The application includes a modern, responsive web interface for managing expenses:

1. Run the API: `dotnet run`
2. Open browser: `https://localhost:5001`
3. Features:
   - ✨ Add expenses with category, amount, and date
   - 📅 Filter expenses by date range
   - 📊 View monthly summary by category
   - 💰 Track total expenses and entry count
   - 🗑️ Delete expenses
   - 📱 Fully responsive design (desktop, tablet, mobile)

## Next Steps

- [x] Create web UI
- [ ] Add budget tracking
- [ ] Add authentication
- [ ] Export to CSV/Excel

## Technologies

- ASP.NET Core 9.0
- Microsoft.Data.SqlClient
- MSSQLLocalDB or AWS RDS MS SQL Server
- Swagger/OpenAPI
