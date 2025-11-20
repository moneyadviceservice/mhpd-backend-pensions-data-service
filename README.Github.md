# Pension Data Service

Pension Data Service is responsible for initiating the entire pension retrieval journey. It is the primary Api for the mhpd dashboard and returns pension arrangement data that the UI is capable of rendering.

## Features

- **Pei Id retrieval**: Fetches a token that contains the Pei Id from the Cda emulator. This Id will be used to fetch all pension arragements associated with the current user.
- **Pension Request orchestration**: Initiates the process of fetching all pension arrangements associated with a given Pei Id.
- **Pension retrieval**: A GET endpoint that can be rouotinely polled to fetch any pension arrangements that are ready to display.
- **Session Management**: Creates a session object that can be used to coordinate downstream services.
- **Logging**: Implements robust logging for tracking requests and errors.

## Architecture

The service follows a modular architecture with clear separation of concerns. Key components include:

- **Controllers**: Handle incoming HTTP requests and orchestrate responses.
- **wwwroot**: Contains a dynamic Open Api spec for this service's Api. This document reflects any changes to the controller's endpoints
- **Models**: Define the structure of data being transmitted and received.
- **Utilities**: Contain reusable functions and helper classes.

## Tech Stack

The Pension Data Service is built using the following technologies:

- **.NET 8.0**: The core framework for building the microservice, supporting modern C# features and performance improvements.
- **C#**: The primary programming language used for service development.
- **MhpdCommon**: A shared library for models and utilities used across the MHPD ecosystem.
- **Azure.Extensions.AspNetCore.Configuration.Secrets**: For accessing secrets in Azure during application configuration.
- **Azure.Identity**: For authenticating with Azure services.
- **Microsoft.Azure.AppConfiguration.AspNetCore**: For managing application settings in Azure App Configuration.
- **Newtonsoft.Json**: For JSON serialization and deserialization.
- **System.Configuration.ConfigurationManager**: For configuration management in .NET applications.
- **System.IdentityModel.Tokens.Jwt**: For handling JWT tokens for authentication.
- **XUnit**: For unit testing the service.
- **Moq**: For mocking dependencies in unit tests.

## Service Dependencies

The MaPSCDAService has the following key service dependencies:

- **Token Integration Service**: Used to obtain Id and Access tokens from the Cda emulator.
- **[Maps Cda Service](https://github.com/moneyadviceservice/mhpd-backend-cda-service)**: Needed to obtain a requesting party token. This enable communications with the token service above
- **[Pension Retrieval Service](https://github.com/moneyadviceservice/mhpd-backend-pensions-retrieval-service)**: Initiates the fetching of pension arrangements associated with a Pei.
- **[Retrieved Pensions Service](https://github.com/moneyadviceservice/mhpd-backend-retrieved-pensions-service)**: Returns any obtianed pension arrangements.

## Installation

To set up the Pension Data Service locally, follow these steps:

1. **Clone the Repository**:
```bash
   git clone https://github.com/moneyadviceservice/mhpd-backend-pensions-data-service.git
   cd app
```

2. **Restore Dependencies**:
```bash
dotnet restore
```

3. **Configure Application Settings**:
```bash
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ApplicationInsights": {
    "ConnectionString": "$(AppInsightsConnString)"
  },
  "PeiRetrievalDuration": 60,
  "ViewDataRetrievalDuration": 5,
  "MapsCdaServiceUrl": "$(MapsCdaServiceUrl)",
  "TokenIntegrationServiceUrl": "$(TokenIntegrationServiceUrl)",
  "PensionRetrievalServiceUrl": "$(PensionRetrievalServiceUrl)",
  "RetrievedPensionsServiceUrl": "$(RetrievedPensionsServiceUrl)",
  "OpenApiServerUrl": "$(OpenApiServerUrl)",
  "CosmosDBConnectionString": "$(CosmosDBConnectionString)",
  "ServiceBusConnectionString": "$(ServiceBusConnectionString)",
  "CosmosBusinessConfiguration": {
    "DatabaseId": "$(DatabaseId)",
    "UserSessionDataContainer": "$(UserSessionDataContainer)"
  },
  "CommonServiceBusConfiguration": {
    "OutboundQueue": "pensions-retrieval-job"
  }
}
# Make sure to replace the placeholder values with actual settings for your environment.
```

4. **Build the Service**:
```bash
dotnet build
```

5. **Run the Service**:
```bash
dotnet run
```


## Testing
Unit tests are implemented to ensure the reliability of the service. To run the tests, navigate to the tests directory and execute:
```bash
cd tests
dotnet test
```

## Logging
Logging is configured to capture detailed information about requests and errors. Logs are written using the ILogger interface, providing insights into the operation of the service


## Contributing
Submit a pull request or open an issue for any enhancements or bug fixes.

## 📦 Release Notes

### 🔧 Release 0.3.0 — 2025-03-04
- Added CSRF support .

### 🔧 Release 0.5.0 — 2025-09-08
- Applied industry standard security response headers.
- Updated logging output consistency to improve traceability.

### 🔧 Release 0.7.0 — 2025-11-04
- HasIncome property of pension retrieval summary now returned as a boolean.
- Removed support for Multiplicity pension arrangements with duplicate payment details.

### 🔧 Release 0.8.0 — 2025-12-03
- Added Support for AVC Pension Type.
- Added Support for linked pensions
- Added Data analytics endpoint