# PEI Integration Service

The PEI Integration Service is a microservice that retrieves PEIs from the CDA service associated with a given user. It ensures that users can access their PEI data securely and efficiently.

## Features

- **PEI Retrieval**: Fetches PEIs using user credentials from the CDA service.
- **Session Management**: Manages user sessions to ensure secure access to PEI data.
- **Validation**: Implements validation of incoming requests to ensure data integrity and security.

## Architecture

The PEI Integration Service follows a modular architecture that promotes separation of concerns. Key components include:

- **Controllers**: Handle incoming HTTP requests and orchestrate responses.
- **Models**: Define the data structures for PEI requests and responses. [WIKI Models](https://mapswiki.atlassian.net/wiki/spaces/MPD/pages/13666675/PeI+Data)
- **Utilities**: Provide reusable functions for common operations.
- **Architecture Diagram HLD**: [MHPD HL Architecture](https://mapswiki.atlassian.net/wiki/spaces/MPD/pages/13666036/High+Level+Architecture)

## Tech Stack

The PEI Integration Service is built using the following technologies:

- **.NET 8.0**: The core framework for building the microservice, supporting modern C# features and performance improvements.
- **C#**: The primary programming language used for development.
- **Microsoft.Azure.Cosmos**: For managing data in Azure Cosmos DB.
- **Microsoft.Extensions.Http**: For facilitating HTTP client communication with external services.
- **Swashbuckle.AspNetCore**: For API documentation and Swagger UI integration.
- **MhpdCommon**: A shared library for models and utilities used across the MHPD ecosystem.
- **XUnit**: For unit testing the service.
- **Moq**: For mocking dependencies in unit tests.

## Service Dependencies

The PEI Integration Service has the following key service dependencies:

- **CDA Service**: Responsible for handling authentication and redirect operations, providing necessary user tokens and session management.
- **Token Integration Service**: Manages token generation and validation for secure interactions.
- **Maps RQP Service**: Integrates with other services to retrieve necessary data for processing requests.

## Installation

To set up the PEI Integration Service locally, follow these steps:

1. **Clone the Repository**:
```bash
   git clone <repository-url>
   cd mhpd-backend/services/PeiIntegrationService
```

2. **Restore Dependencies**: Ensure that you have the .NET SDK installed (version 6.0 or later)
```bash
dotnet restore
```

3. **Configure Application Settings**: Update the appsettings.json file to set your configuration settings. Here’s an example configuration:
```bash
{
  "CdaServiceUrl": "<CdaPeisServiceEndpoint>",
  "MapsCdaServiceUrl": "<MapsCdaServiceEndpoint>",
  "TokenIntegrationServiceUrl": "<TokenIntegrationServiceEndpoint>"
    "Logging": {
    "LogLevel": {
      "Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware": "Information",
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
# Make sure to replace the placeholder values with actual settings for your environment.
```

4. **Build the Service**:
```bash
dotnet build
```

4. **Run the Service**:
```bash
dotnet run
```

## Testing
Unit tests are implemented to ensure the reliability of the service. To run the tests, navigate to the tests directory and execute:
```bash
dotnet test
```

## Contributing
Submit a pull request or open an issue for any enhancements or bug fixes.