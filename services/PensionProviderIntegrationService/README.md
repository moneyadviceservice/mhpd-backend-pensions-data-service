# Pension Provider Integration Service

The Pension Provider Integration Service is an Azure Function app designed to interact with external pension providers and manage the retrieval of pension-related information. It provides a standardized interface for making requests to various pension data sources.

## Features

- **PEI Retrieval**: Interfaces with external pension providers to retrieve PEIs.
- **Session Management**: Manages user sessions for secure access to pension data.
- **Error Handling**: Robust error handling for various scenarios encountered during data retrieval.

## Architecture

The service is designed with a modular architecture that promotes separation of concerns. Key components include:

- **Functions**: Handle incoming requests and orchestrate responses.
- **Models**: Define the data structures for requests and responses, including response models for handling API data.
- **HttpClient**: Encapsulates the logic for making HTTP requests to external services.

## Tech Stack

The Pension Provider Integration Service is built using the following technologies:

- **.NET 8.0**: The core framework for building the microservice, supporting modern C# features and performance improvements.
- **Azure Functions v4**: The framework used for implementing serverless functions.
- **C#**: The primary programming language used for service development.
- **Azure.Messaging.ServiceBus**: For managing communication with Azure Service Bus.
- **MhpdCommon**: A shared library for models and utilities used across the MHPD ecosystem.
- **Microsoft.Azure.Cosmos**: For managing data in Azure Cosmos DB.
- **Microsoft.Azure.Functions.Extensions**: Provides extension methods for Azure Functions.
- **Microsoft.Azure.Functions.Worker**: For building Azure Functions using the Worker SDK.
- **Microsoft.Azure.Functions.Worker.Extensions.Http**: For handling HTTP triggers in Azure Functions.
- **Microsoft.ApplicationInsights.WorkerService**: For monitoring and logging with Application Insights.
- **Polly**: For implementing resilience and transient-fault-handling capabilities.
- **System.IdentityModel.Tokens.Jwt**: For handling JWT tokens for authentication.

## Service Dependencies

The Pension Provider Integration Service has the following key service dependencies:

- **CDA Service**: Retrieves Pension Eligibility Information based on user credentials.
- **Maps RQP Service**: Integrates with other services to retrieve necessary data for processing requests.
- **Token Integration Service**: Manages token generation and validation for secure interactions.

## Installation

To set up the Pension Provider Integration Service locally, follow these steps:

1. **Clone the Repository**:
   ```bash
   git clone <repository-url>
   cd mhpd-backend/services/PensionProviderIntegrationService
	```

2. **Restore Dependencies**:
```bash
dotnet restore
```

3. **Configure Application Settings**: Update the local.settings.json file with your configuration settings. Here’s an example structure based on the current requirements:
```bash
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "<your_storage_connection_string>",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "MapsCdaServiceUrl": "<your_maps_cda_service_url>",
    "TokenIntegrationServiceUrl": "<your_token_integration_service_url>",
    "CdaServiceUrl": "<your_cda_service_url>",
    "DatabaseId": "mhpd-businesslayer",
    "ContainerId": "mhpdholdernameViewConfigurationData",
    "ContainerPartitionKey": "/holdernameGuid",
    "CosmosDBConnectionString": "AccountEndpoint=<your_cosmosdb_account_endpoint>;AccountKey=<your_cosmosdb_account_key>;",
    "ServiceBusConnectionstring": "Endpoint=sb://<your_servicebus_namespace>.servicebus.windows.net/;SharedAccessKeyName=<your_shared_access_key_name>;SharedAccessKey=<your_shared_access_key>;",
    "InboundQueue": "pension-details-request",
    "OutboundQueue": "retreived-pension-details"
  },
  "ConnectionStrings": {
    "CosmosDBConnectionString": "AccountEndpoint=<your_cosmosdb_account_endpoint>;AccountKey=<your_cosmosdb_account_key>;",
    "ServiceBusConnectionstring": "Endpoint=sb://<your_servicebus_namespace>.servicebus.windows.net/;SharedAccessKeyName=<your_shared_access_key_name>;SharedAccessKey=<your_shared_access_key>;"
  }
}
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
dotnet test
```

## Contributing
Submit a pull request or open an issue for any enhancements or bug fixes.