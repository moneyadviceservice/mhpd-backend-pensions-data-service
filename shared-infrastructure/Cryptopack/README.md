# Pension Retrieval Service

The Cryptopack processor is an Azure Function app designed to unwrap a compressed zip file containing a known manifest of files and validating them. 
It can also upload the contents of some of those files to the key vault in Azure and restart affacted services.
A link to the full description of the implementation and configuration can be found [here](https://mapswiki.atlassian.net/wiki/spaces/MPD/pages/222527590/Cryptopack+Processing+and+Configuration)

## Features

- **Manifest Validation**: Verifies that all expected files are present and in the correct format.
- **Certficate Generation**: Combines certificate files into a chain and signs them with a specified private key file.
- **Key Vault Upload**: Imports secrets and certificates into the key vault.
- **Application Service Restart**: Updates the configuration of services and restarts them.

## Tech Stack

The Cryptopack processor is built using the following technologies:

- **.NET 8.0**: The core framework for building the microservice, supporting modern C# features and performance improvements.
- **Azure Functions v4**: The framework used for implementing serverless functions.
- **C#**: The primary programming language used for service development.
- **Azure.Storage.Blob**: Allows it to upload the unwrapped contents of the zip file to an archive.
- **Azure.Security.KeyVault.Secrets**: Used to import screts into the key vault.
- **Azure.Security.KeyVault.Certificates**: Used to import certificates into the key vault.
- **Microsoft.Azure.Functions.Worker**: For building Azure Functions using the Worker SDK.
- **Azure.ResourceManager.AppService**: Used to access and modify the configuration of an application in Azure.
- **Microsoft.ApplicationInsights.WorkerService**: For monitoring and logging with Application Insights.

## Installation

To set up the Pension Retrieval Service locally, follow these steps:

1. **Clone the Repository**:
   ```bash
   git clone <repository-url>
   cd mhpd-backend/shared-infrastructure/Cryptopack/app
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
		  "AzureWebJobsStorage": "UseDevelopmentStorage=true",
		  "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
		  "StorageConnectionString": "UseDevelopmentStorage=true"
		},
		"Manifest": {
		  "KeyId": "kid",
		  "MtlsCertificate": "certificate",
		  "MtlsChain": "certificateChain",
		  "CertificatePair": {
			"PrivateKey": "certPrivateKey",
			"PublicKey": "certPublicKey",
			"AlgorithmType": 1
		  },
		  "JwtPair": {
			"PrivateKey": "jwtPrivateKey",
			"PublicKey": "jwtPublicKey",
			"AlgorithmType": 0
		  }
		},
		"KeyVaultSettings": {
		  "KeyVaultUrl": "<key-vault-url>",
		  "TenantId": "<service-principal-tenantId>",
		  "ClientId": "<service-principal-objectId>",
		  "ClientSecret": "<service-principal-secret>"
		},
		"CryptopackSettings": {
		  "PfxPassword": "<password>",
		  "MtlsCertificateName": "<Name-for-uploaded-certificate>",
		  "PrivateKeySecretName": "<Name-for-uploaded-PrivateKey>",
		  "KidSecretName": "<Name-for-uploaded-kid>"
		},
		"WebAppSettings": {
		  "AppName": "CDA-service-dev",
		  "SubscriptionId": "<SubscriptionId>",
		  "ResourceGroupName": "<ResourceGroup>",
		  "JwtKeyVariable": "<Name-for-private-key-variable>",
		  "JwtKidVariable": "<Name-for-kid-variable>"
		}
	}
# Make sure to replace the placeholder values with actual settings for your environment.
```

## Contributing
Submit a pull request or open an issue for any enhancements or bug fixes.
