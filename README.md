# BookHub – Private Book Library System

BookHub is a complete online book retail and in-store pickup platform built with **ASP.NET Core Web API** and **Razor Pages**. It supports secure member logins, catalog filtering, cart and order management, bookmarks, and an admin dashboard for managing books, discounts, and announcements.

<img width="1895" height="1002" alt="book-library-store" src="https://github.com/user-attachments/assets/96bba960-c6ca-403f-ad0e-4f3960f0c99c" />


---

## Tech Stack

| Layer       | Tech                                    |
|-------------|-----------------------------------------|
| API         | ASP.NET Core 8 Web API (`LibraryAPI`)   |
| Front End   | Razor Pages MVC Web App (`LibraryWeb`)  |
| Database    | Entity Framework Core with SQL Server   |
| Auth        | JWT (Admin/Member Roles)                |
| UI Styling  | CSS/Bootstrap 5                         |

---

## Features

- JWT-based member login
- Filterable catalog: genre, author, availability, price, language, format
- Bookmark system (whitelist)
- Cart & order system with claim code billing
- Order history & cancelation support
- Admin inventory & book management
- Discount pricing with sale periods
- Timed admin announcements (e.g., deals, new arrivals)

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022+ with ASP.NET and EF Core workloads
- SQL Server (Express or LocalDB)
- EF Core CLI tools:
  ```bash
  dotnet tool install --global dotnet-ef
  

**Running the Project**

**1. Open the Solution**
Open BookHub.sln in Visual Studio.

**2. Set Both Projects to Run Simultaneously**
Right-click the Solution (BookHub) > Set Startup Projects

Choose Multiple startup projects: Set both LibraryAPI and LibraryWeb to Start
This ensures pressing F5 runs both backend and frontend together.

**3. Configure SQL Server Connection**
In LibraryAPI/appsettings.json, set your database connection:

"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;         Database=BookHubDB;Trusted_Connection=True;TrustServerCertificate=True;"
}

Replace the Server name if needed (e.g. localhost\\SQLEXPRESS, or your machine's SQL instance).


**4. Apply Migrations (First Run Only)**

To generate database tables:
```
cd LibraryAPI
dotnet ef database update
```

This applies the latest schema migration and creates BookHubDB.

**5. Run the App**
From Visual Studio:
Press F5 (or Ctrl + F5) and both projects will run.

Or run individually:

```cd LibraryAPI
dotnet run

cd ../LibraryWeb
dotnet run
API: https://localhost:7110

Web: https://localhost:7171
```

**Admin Setup**
By default, all registered users are Members. To promote one to Admin, run this SQL:

UPDATE Members SET IsAdmin = 1 WHERE Email = 'admin@example.com';


In the appsettings.json, also update this SMTP key to your own gmail email and app password. This will be used to send email confirmation to registered user's inbox.

"SmtpSettings": {
  "Host": "smtp.gmail.com",
  "Port": 587,
  "Username": "laibarystorer@gmail.com",
  "Password": "asdasdasdasd" //gmail smtp requires app password. this is the app password for 'SMTP Mail' app created from security options
},



📂 **Project Structure**

```BookHub/

├── LibraryAPI/         # ASP.NET Core Web API backend
│   ├── Controllers/    # Books, Auth, Orders, Admin APIs
│   └── Models/         # EF Core entities (Book, Order, etc.)
├── LibraryWeb/         # Razor Pages MVC frontend
│   ├── Controllers/    # UI controllers (Books, Cart, Orders)
│   └── Views/          # Razor views (UI)
```

🧾 Admin Seeder (Optional)
Add this to your Program.cs (API side) to seed an admin user:

```
if (!context.Members.Any())
{
    context.Members.Add(new Member
    {
        FullName = "Admin",
        Email = "admin@example.com",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
        IsAdmin = true,
        MembershipId = Guid.NewGuid().ToString().Substring(0, 8).ToUpper()
    });
    context.SaveChanges();
}
```
Then use:

Email: admin@example.com
Password: Admin@123

📝 License

MIT — free to use, modify, and distribute.
