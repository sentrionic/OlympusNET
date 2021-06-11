# OlympusNET

OlympusNET is a backend for the [OlympusBlog](https://github.com/sentrionic/OlympusBlog) using [.NET 5](https://dotnet.microsoft.com/).

## Stack

- Using Clean Architecture and CQRS and MediatR
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/) as the database ORM
- [ImageSharp](https://github.com/SixLabors/ImageSharp) for image resizing
- [FluentValidation](https://fluentvalidation.net/) for validation

## Getting started

0. Install [.NET 5.0](https://dotnet.microsoft.com/download)
1. Clone this repository
2. Install Postgres and Redis.
3. Open the project in Visual Studio to get all the dependencies.
4. Rename `appsettings.Development.example` in `API` to `appsettings.Development`
   and fill out the values. AWS is only required if you want file upload,
   GMail if you want to send reset emails.
5. Run `dotnet run -p API`.
6. Go to `localhost:5000/swagger/v1/swagger.json` for a list of all the endpoints.
