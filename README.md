Hybrid Application Tracking System (HATS) is a full-featured web API built with ASP.NET Core 8.0 and Entity Framework Core designed to manage job applications with automated and manual workflows. It supports three distinct roles: Applicants, Admins, and BotMimic automation for technical applications.

Features
JWT-based Authentication and Role-based Authorization

Applicant can apply to jobs and track their application status

Admin can manually manage job postings and non-technical applications

BotMimic automates application status progression for technical roles

Database persistence with SQL Server and EF Core migrations

Swagger UI integration for easy API testing

Technologies Used
.NET 8.0 / ASP.NET Core Web API

Entity Framework Core (Code First)

SQL Server / LocalDB

Swagger (Swashbuckle)

C#

Getting Started
Prerequisites
Visual Studio 2022 or later

.NET 8.0 SDK

SQL Server or LocalDB instance

Running the Project
Clone this repository.

Update connection string in appsettings.json.

Use Package Manager Console:

text
Add-Migration InitialCreate
Update-Database
Press F5 to run the application. Swagger will be available at:

text
https://localhost:5001/swagger
Default Users for Testing
Username	Password	Role
admin	password3	Admin
applicant1	password1	Applicant
botmimic	password2	BotMimic
API Endpoints (Sample)
POST /Auth/login — Login and get JWT token

POST /Applicant/apply — Applicant submits job application

GET /Applicant/my-applications — List applicant's applications

GET /Admin/all-applications — Admin views all applications

PUT /Admin/update-application-status/{id} — Admin updates non-technical app status

POST /BotMimic/process-technical-applications — Bot processes technical apps through stages

Project Structure
text
/Controllers
/Data
/Models
/Program.cs
/appsettings.json
