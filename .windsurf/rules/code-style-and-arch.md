---
trigger: always_on
---

# About the Application
- The Application is an API Microservice writtern in .NET 9.0
- However, there are is web page servered directly by app i.e. /setup.
- Application Name is OXDesk which provides Helpdesk Tool and Customer Surveys and leverage LLM/AI using external providers mainly OpenAI.

# General Code Style & Formatting
- Use English for all code and documentation.
- Use PascalCase for class and public method names
- Use camelCase for method parameters and local variables.
- Follow the Clean Architecture with Core, Application, Infrasture and API Projects.
- Add XML comments in all interfaces classes and methods. Use inheritdoc to inherit comments from base classes and interfaces in the concrete implementation.
- Add OpenAPI comments to generate XML file for Swagger in all API controllers and endpoints.
- Avoid adding comments to each function or logic

# Project Structure & Architecture
- Core project include the Domain Models, Entities, Value Objects, , Specifications, Interfaces, DTOs, Constants, etc.
- All common functionalities are under Common folder in Core Project.
- All specific features are directly added into their respective folder.
- Data folder for database access related code
- Entity Configurations are stored in respective Data/feature folder under Infrasture Project 
- Application project contains business logic
- API project contains API endpoints using Contollers and Factory Services
- Factory Service is responsible for DTOs transformation.
- Validators folder in API project contains validation logic using FluentValidation
- All Configuration are in Configurations folder under OXDesk.Api project with specific json files for different modules.
- Middlewares are stored in src\OXDesk.Api\Middleware
- Shared functionalities are implemented in Shared Project.
- We have specific project for Tenant, Identity, etc. We need each domain to have separate projects for easy maintanance.

# Functions & Logic
- API Endpoints call the Application Service layer and Application  Services layer calls the Infrastructure layer.
- Use Extension method instead of direct implementation in program.cs
- Use DTOs in the Controller Method for Requests and Responses.
- middleware that automatically adds the api prefix, so controller routes do not need to include it.
- Use Dotnet DateTime.UtCNow for getting current time instead of just .Now
- Use Guid.CreateVersion7() i.e. UUID v7 always
- CacheService to be used for Cache management

# Data Handling
- Use EF Core and using PostgreSQL Database 
- Backend connects to Database and Memory Cache
- Where application, we need to cache the DB response in memory
- UnitOfWork is used for transatin with Start and Commit methods
- No need to pass tenant_id explicitly instead we use EF Core Filters & Overriding SaveChangesAsync

# Caching
- Caching can be implemented in the service layer only for Tenant Data.
- Use existing CacheService which store data in Cache using tenant id as a key

# Response
- Repository is only responsible for Data retrivel and no data manipulation in repository
- Service layer control the business logic
- Factory Service is used for transforming DB results into DTOs with related objects.

# Tests
- Currently only Unit Tests are implmemented
- Tests are tests\OXDesk.Tests