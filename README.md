# Application Tracking System

## Overview

This is a role-based web API built with ASP.NET Core and Entity Framework Core to manage job applications. It supports Applicants, Admins, and an automated Bot Mimic to handle technical applications.

Features include:
- JWT authentication and role-based authorization
- Applicants can apply for jobs and view application statuses
- Admins can manage job postings and manually update applications for non-technical roles
- Bot Mimic automatically processes technical applications through defined stages
- Persistent data storage with SQL Server using EF Core

## Technologies Used

- ASP.NET Core 8.0 Web API  
- Entity Framework Core  
- SQL Server / LocalDB  
- JWT for authentication  
- Swagger for API documentation and testing

## Setup Instructions

1. Clone the repository  
2. Configure your SQL Server connection string in `appsettings.json`  
3. Open Package Manager Console and run:
4. Run the application (`F5` or `dotnet run`), Swagger UI will be available at `https://localhost:5001/swagger`.

## Default Users

| Username   | Password  | Role      |
|------------|-----------|-----------|
| admin      | password3 | Admin     |
| applicant1 | password1 | Applicant |
| botmimic   | password2 | BotMimic  |

## Key API Endpoints

- **Authentication**  
`POST /Auth/login` - login and receive JWT token

- **Applicant**  
`POST /Applicant/apply` - apply for jobs  
`GET /Applicant/my-applications` - list applicant applications  
`GET /Applicant/dashboard` - applicant dashboard

- **Admin**  
`GET /Admin/all-applications` - list all applications  
`PUT /Admin/update-application-status/{id}` - update non-technical applications  
`POST /Admin/create-job-posting` - add job posting  
`GET /Admin/dashboard` - admin dashboard statistics

- **Bot Mimic**  
`POST /BotMimic/process-technical-applications` - move Applied → Under Review  
`POST /BotMimic/schedule-technical-assessment` - Under Review → Technical Assessment  
`POST /BotMimic/move-to-interview` - Technical Assessment → Interview  
`POST /BotMimic/generate-offers` - Interview → Offer  
`GET /BotMimic/statistics` - bot processing stats



