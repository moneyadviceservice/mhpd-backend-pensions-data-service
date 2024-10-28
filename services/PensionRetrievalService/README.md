# Pension Retrieval Service

The Pension Retrieval Service is an Azure Function app designed to manage the retrieval of PEIs from external sources. It provides a reliable interface for making requests and processing responses related to pension data.

## Features

- **PEI Retrieval**: Handles the retrieval of PEIs from various providers.
- **Data Management**: Manages pension retrieval records and ensures data integrity.
- **Error Handling**: Robust error handling for various scenarios encountered during data retrieval.

## Architecture

The service follows a modular architecture with clear separation of concerns. Key components include:

- **Functions**: Implement Azure Functions that handle incoming HTTP requests and orchestrate responses.
- **Models**: Define the data structures for requests and responses, including models for handling API data. [WIKI Models](https://mapswiki.atlassian.net/wiki/spaces/MPD/pages/13666058/Pensions+Retrieval+Record)
- **Repository**: Encapsulates the logic for data access, managing interactions with the Cosmos DB.
- **Architecture Diagram HLD**: [MHPD HL Architecture](https://mapswiki.atlassian.net/wiki/spaces/MPD/pages/13666036/High+Level+Architecture)

## Tech Stack

The Pension Retrieval Service is built using the following technologies:

- **.NET 8.0**: The core framework for building the microservice, supporting modern C# features and performance improvements.
- **Azure Functions v4**: The framework used for implementing serverless functions.
- **C#**: The primary programming language used for service development.
- **Azure.Messaging.ServiceBus**: For managing communication with Azure Service Bus.
- **MhpdCommon**: A shared library for models and utilities used across the MHPD ecosystem.
- **Microsoft.Azure.Cosmos**: For managing data in Azure Cosmos DB.
- **Microsoft.Azure.Functions.Worker**: For building Azure Functions using the Worker SDK.
- **Microsoft.Azure.Functions.Worker.Extensions.Http**: For handling HTTP triggers in Azure Functions.
- **Microsoft.Azure.Functions.Worker.Extensions.ServiceBus**: For handling Service Bus triggers in Azure Functions.
- **Microsoft.ApplicationInsights.WorkerService**: For monitoring and logging with Application Insights.
- **Newtonsoft.Json.Schema**: For JSON schema validation and manipulation.
- **Polly**: For implementing resilience and transient-fault-handling capabilities.
- **Microsoft.VisualStudio.Azure.Containers.Tools.Targets**: For working with Docker containers in Azure Functions.

## Service Dependencies

The Pension Retrieval Service has the following key service dependencies:

- **CDA Service**: Interacts with the CDA service to retrieve Pension Eligibility Information based on user credentials.
- **Maps RQP Service**: Integrates with other services to retrieve necessary data for processing requests.
- **Token Integration Service**: Manages token generation and validation for secure interactions.

## Installation

To set up the Pension Retrieval Service locally, follow these steps:

1. **Clone the Repository**:
   ```bash
   git clone <repository-url>
   cd mhpd-backend/services/PensionRetrievalService
	```
2. **Restore Dependencies**:
	```bash
	dotnet restore
	```

3. **Configure Application Settings**:
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
    "ContainerId": "mhpdPensionsRetrievalRecords",
    "ContainerPartitionKey": "/userSessionId",
    "CosmosDBConnectionString": "AccountEndpoint=<your_cosmosdb_account_endpoint>;AccountKey=<your_cosmosdb_account_key>;",
    "ServiceBusConnectionstring": "Endpoint=sb://<your_servicebus_namespace>.servicebus.windows.net/;SharedAccessKeyName=<your_shared_access_key_name>;SharedAccessKey=<your_shared_access_key>;",
    "InboundQueue": "pensions-retrieval-job",
    "OutboundQueue": "pension-details-request"
  },
  "ConnectionStrings": {
    "CosmosDBConnectionString": "AccountEndpoint=<your_cosmosdb_account_endpoint>;AccountKey=<your_cosmosdb_account_key>;",
    "ServiceBusConnectionstring": "Endpoint=sb://<your_servicebus_namespace>.servicebus.windows.net/;SharedAccessKeyName=<your_shared_access_key_name>;SharedAccessKey=<your_shared_access_key>;"
  }
}
# Make sure to replace the placeholder values with actual settings for your environment.
```

## Testing
Unit tests are implemented to ensure the reliability of the service. To run the tests, navigate to the tests directory and execute:
```bash
dotnet test
```

## Contributing
Submit a pull request or open an issue for any enhancements or bug fixes.
