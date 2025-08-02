# NiyaCRM.AppInstallation

This project contains components for managing application installation, data seeding, and version upgrades outside the scope of EF Migrations.

## AppInstallationStatus Model

The `AppInstallationStatus` model is designed to track the status of installation steps or pipeline processes:

- **Pipeline**: Varchar(15) - The pipeline name or identifier
- **Step**: Varchar(50) - The step name or description within the pipeline
- **Completed**: Varchar(1) - Indicates whether the step is completed ('Y' = Completed, 'N' = Not Completed)

## Registering the AppInstallationStatus Model in DbContext

To register the `AppInstallationStatus` model in your DbContext without creating circular dependencies, follow these steps:

### Step 1: Add the DbSet to ApplicationDbContext

In your `ApplicationDbContext.cs` file in the Infrastructure project, add:

```csharp
public DbSet<AppInstallationStatus> AppInstallationStatuses { get; set; } = null!;
```

### Step 2: Update OnModelCreating Method

In the `OnModelCreating` method of your `ApplicationDbContext`, add:

```csharp
// Add this line to register AppInstallation models
builder.AddAppInstallationModels();
```

### Step 3: Add the Using Statement

Add the following using statement at the top of your `ApplicationDbContext.cs` file:

```csharp
using NiyaCRM.AppInstallation;
using NiyaCRM.AppInstallation.Models;
```

## Creating and Using Migrations

After registering the model, create a new migration to add the AppInstallationStatus table:

```powershell
dotnet ef migrations add AddAppInstallationStatus -p src\NiyaCRM.Infrastructure -s src\NiyaCRM.Api
```

Then apply the migration:

```powershell
dotnet ef database update -p src\NiyaCRM.Infrastructure -s src\NiyaCRM.Api
```

## Using the AppInstallationStatus Model

You can use the AppInstallationStatus model to track the progress of installation steps:

```csharp
// Example: Creating a new installation status record
var status = new AppInstallationStatus
{
    Id = Guid.NewGuid(),
    Pipeline = "Installation",
    Step = "Database Setup",
    Completed = "N",
    CreatedAt = DateTime.UtcNow,
    CreatedBy = userId
};

_dbContext.AppInstallationStatuses.Add(status);
await _dbContext.SaveChangesAsync();

// Example: Updating a status to completed
status.Completed = "Y";
status.UpdatedAt = DateTime.UtcNow;
status.UpdatedBy = userId;
await _dbContext.SaveChangesAsync();
```
