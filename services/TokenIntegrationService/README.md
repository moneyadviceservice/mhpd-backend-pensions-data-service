# Token Integration Service

The Token Integration Service is a microservice designed to manage token generation and validation. It provides a reliable interface for processing token requests and interacting with external systems.

## Features

- **Token Generation**: Handles the generation of tokens based on user requests.
- **Token Validation**: Validates incoming tokens to ensure security and compliance.
- **Error Handling**: Implements robust error handling for various scenarios encountered during token processing.

## Architecture

The service follows a modular architecture with clear separation of concerns. Key components include:

- **Controllers**: Implement Azure Functions that handle incoming HTTP requests and orchestrate responses.
- **Models**: Define the data structures for requests and responses, including models for handling token requests and responses.
- **HttpClients**: Encapsulates the logic for interacting with external services related to token management.
- **Architecture Diagram HLD**: [MHPD HL Architecture](https://mapswiki.atlassian.net/wiki/spaces/MPD/pages/13666036/High+Level+Architecture)

## Tech Stack

The Token Integration Service is built using the following technologies:

- **.NET 8.0**: The core framework for building the microservice, supporting modern C# features and performance improvements.
- **C#**: The primary programming language used for service development.
- **System.IdentityModel.Tokens.Jwt**: For handling JWT tokens for authentication and authorization.
- **MhpdCommon**: A shared library for models and utilities used across the MHPD ecosystem.
- **Microsoft.Extensions.Http**: For simplifying HTTP requests and managing dependencies related to HTTP clients.
- **Swashbuckle.AspNetCore**: For generating Swagger documentation for API endpoints.
- **Microsoft.Azure.Cosmos**: For managing data in Azure Cosmos DB.
- **Newtonsoft.Json**: For JSON serialization and deserialization.

## Service Dependencies

The Token Integration Service has the following key service dependencies:

- **CDA Service**: Interacts with the CDA service to validate tokens and retrieve necessary information.

## Installation

To set up the Token Integration Service locally, follow these steps:

1. **Clone the Repository**:
   ```bash
   git clone <repository-url>
   cd mhpd-backend/services/TokenIntegrationService
	```

2. **Restore Dependencies**: Ensure that you have the .NET SDK installed (version 6.0 or later)
```bash
dotnet restore
```

3. **Configure Application Settings**: Update the appsettings.json file to set your configuration settings. Here’s an example configuration:
```bash
{
  "Logging": {
    "LogLevel": {
      "Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware": "Information",
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "CdaServiceUrl": "$(cdaServiceEndpoint)"
}
```

## Testing
Unit tests are implemented to ensure the reliability of the service. To run the tests, navigate to the tests directory and execute:
```bash
dotnet test
```

## Contributing
Submit a pull request or open an issue for any enhancements or bug fixes.
