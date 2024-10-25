# MaPSCDAService

MaPSCDAService is a microservice designed for handling requests related to user authentication and redirect management in the MHPD. This service is part of a larger backend architecture aimed at facilitating secure access to user data and services.

## Features

- **Token Generation**: Generates secure tokens for user sessions using PKCE (Proof Key for Code Exchange).
- **Redirect Management**: Manages redirect requests and responses based on user authentication details.
- **Validation**: Ensures that incoming requests contain valid data before processing.
- **Logging**: Implements robust logging for tracking requests and errors.

## Architecture

The service follows a modular architecture with clear separation of concerns. Key components include:

- **Controllers**: Handle incoming HTTP requests and orchestrate responses.
- **Models**: Define the structure of data being transmitted and received. [WIKI Models](https://mapswiki.atlassian.net/wiki/spaces/MPD/pages/13666559/Models)
- **Utilities**: Contain reusable functions and helper classes.
- **Wiki Link**: [MapsCDAService Architecture](https://mapswiki.atlassian.net/wiki/spaces/MPD/pages/117735497/MaPS+CDA+Service)
- **Architecture Diagram HLD**: [MHPD HL Architecture](https://mapswiki.atlassian.net/wiki/spaces/MPD/pages/13666036/High+Level+Architecture)

## Tech Stack

The MaPSCDAService is built using the following technologies:

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

- **CDA Service**: Responsible for handling authentication and redirect operations, providing necessary user tokens and session management.
- **Tokens Integration Service**: Facilitates the generation and management of tokens required for secure interactions.
- **PEI Integration Service**: Retrieves PEIs from the CDA service associated with a given user.
- **Pension Provider Integration Service**: Connects to external pension data providers to fetch and manage pension-related information.
- **Pensions Retrieval Service**: Handles requests for retrieving pension data from various sources and services.
- **Pensions Retrieval Job Queue**: A queue system for managing asynchronous retrieval requests related to pension data.
- **Retrieved Pensions Record Service**: A service dedicated to managing and returning retrieved pension records.

These dependencies are crucial for the functionality and operation of the MaPSCDAService, allowing it to interact with other services in the MHPD ecosystem effectively.

## Installation

To set up the MaPSCDAService locally, follow these steps:

1. **Clone the Repository**:
```bash
   git clone <repository-url>
   cd mhpd-backend/services/MaPSCDAService
```

2. **Restore Dependencies**:
```bash
dotnet restore
```

3. **Configure Application Settings**:
```bash
{
  "UriSettings": {
    "RedirectTargetUrl": "https://your.redirect.url"
  },
  "Kid": "your-key-id",
  "Audience": "your-audience"
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

## MHPD Common Usage

The MaPSCDAService depends on the MHPD Common library, which provides shared models and utilities used throughout the MHPD ecosystem.

### Adding MHPD Common as a Dependency

To use MHPD Common in your project, follow these steps:

1. **Install the Package: Add the MHPD Common library to your project via NuGet. You can do this by running the following command**:
```bash
dotnet add package MhpdCommon --version <version-number>dotnet add package MhpdCommon --version <version-number>
```

2. **Import Namespaces: In your code files, ensure to include the necessary namespaces**:
```bash
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Extensions;
```

3. **Usage Notes**
- **Models**: Utilize the models provided by MHPD Common for data interchange between services. For example, use MHPDModels for standard user and session-related data.
- **Utilities**: Take advantage of the extension methods and utility functions in the MHPD Common library to simplify common tasks, such as validation and logging.

## Testing
Unit tests are implemented to ensure the reliability of the service. To run the tests, navigate to the tests directory and execute:
```bash
dotnet test
```

## Logging
Logging is configured to capture detailed information about requests and errors. Logs are written using the ILogger interface, providing insights into the operation of the service

## Contributing
Submit a pull request or open an issue for any enhancements or bug fixes.