# Retrieved Pensions Record Service

The Retrieved Pensions Record Service is an Azure Function app designed to manage and retrieve pension records from external sources. It provides a reliable interface for processing requests and managing the associated pension data.

## Features

- **Pension Record Retrieval**: Handles the retrieval of pension records based on user queries.
- **Data Management**: Manages the storage and integrity of retrieved pension records.
- **Error Handling**: Implements robust error handling for various scenarios encountered during data retrieval.

## Architecture

The service follows a modular architecture with clear separation of concerns. Key components include:

- **Functions**: Implement Azure Functions that handle incoming HTTP requests and orchestrate responses.
- **Models**: Define the data structures for requests and responses, including models for handling pension records. [WIKI Models](https://mapswiki.atlassian.net/wiki/spaces/MPD/pages/13666799/Retrieved+Pension+Record)
- **Repository**: Encapsulates the logic for data access, managing interactions with the Cosmos DB.
- **Architecture Diagram HLD**: [MHPD HL Architecture](https://mapswiki.atlassian.net/wiki/spaces/MPD/pages/13666036/High+Level+Architecture)

## Tech Stack

The Retrieved Pensions Record Service is built using the following technologies:

- **.NET 8.0**: The core framework for building the microservice, supporting modern C# features and performance improvements.
- **Azure Functions v4**: The framework used for implementing serverless functions.
- **C#**: The primary programming language used for service development.
- **MhpdCommon**: A shared library for models and utilities used across the MHPD ecosystem.
- **Microsoft.Azure.Cosmos**: For managing data in Azure Cosmos DB.
- **Microsoft.Azure.Functions.Extensions**: Provides extension methods for Azure Functions.
- **Microsoft.Azure.Functions.Worker**: For building Azure Functions using the Worker SDK.
- **Microsoft.Azure.Functions.Worker.Extensions.Http**: For handling HTTP triggers in Azure Functions.
- **Microsoft.Azure.Functions.Worker.Extensions.ServiceBus**: For handling Service Bus triggers in Azure Functions.
- **Microsoft.ApplicationInsights.WorkerService**: For monitoring and logging with Application Insights.
- **Newtonsoft.Json.Schema**: For JSON schema validation and manipulation.
- **Microsoft.VisualStudio.Azure.Containers.Tools.Targets**: For working with Docker containers in Azure Functions.

## Installation

To set up the Retrieved Pensions Record Service locally, follow these steps:

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
    "AzureWebJobsStorage": "<your_azure_web_jobs_storage>",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "DatabaseId": "mhpd-businesslayer",
    "ContainerId": "mhpdRetrievedPensionRecords",
    "ContainerPartitionKey": "pensionsRetrievalRecordId",
    "InboundQueue": "retreived-pension-details",
    "OutboundQueue": "",
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
