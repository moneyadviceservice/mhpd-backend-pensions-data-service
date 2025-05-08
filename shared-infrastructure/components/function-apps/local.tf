locals {
  enable_vnet_integration = var.env == "staging" || var.env == "prod" ? true : false
  subnet_id               = length(data.azurerm_subnet.mhpd-subnet) > 0 ? data.azurerm_subnet.mhpd-subnet["exists"].id : null
  sku_name                = var.env == "prod" ? "EP2" : "EP1"
  zone_redundant          = var.env == "prod" ? true : false
  # create_service_plan     = var.env == "staging" || var.env == "prod" ? true : false
  pension_request_app_settings = {
    "APPLICATIONINSIGHTS_CONNECTION_STRING"                    = "InstrumentationKey=${module.pension_request_function.instrumentation_key};IngestionEndpoint=https://uksouth-1.in.applicationinsights.azure.com/;LiveEndpoint=https://uksouth.livediagnostics.monitor.azure.com/;ApplicationId=${module.pension_request_function.app_insights_app_id}"
    "CdaServiceUrl"                                            = "https://maps-api-management-${var.env}.azure-api.net/cda-integration-external/"
    "PdpServiceUrl"                                            = "https://maps-api-management-${var.env}.azure-api.net/view-data-external/view-data/"
    "TokenIntegrationServiceUrl"                               = "https://maps-api-management-${var.env}.azure-api.net/token-integration-service/"
    "MapsCdaServiceUrl"                                        = "https://maps-api-management-${var.env}.azure-api.net/maps-cda-service/"
    "FUNCTIONS_EXTENSION_VERSION"                              = "~4"
    "FUNCTIONS_WORKER_RUNTIME"                                 = "dotnet-isolated"
    "WEBSITE_CONTENTSHARE"                                     = "pensionrequestfunctionaee5"
    "WEBSITE_RUN_FROM_PACKAGE"                                 = "1"
    "WEBSITE_USE_PLACEHOLDER_DOTNETISOLATED"                   = "1"
    "CosmosIntegrationConfiguration__DatabaseId"               = "mhpd-integration-layer"
    "CosmosIntegrationConfiguration__HolderNameCacheContainer" = "mhpdholderNameViewConfigurationData"
    "CosmosIntegrationConfiguration__JwkCacheContainer"        = "jwkUriEmulatorData"
    "CosmosBusinessConfiguration__DatabaseId"                  = "mhpd-business-layer"
    "CosmosBusinessConfiguration__UserSessionDataContainer"    = "mhpdUserSessionData"
    "CommonServiceBusConfiguration__InboundQueue"              = "pension-details-request"
    "CommonServiceBusConfiguration__OutboundQueue"             = "retrieved-pension-details"
    # "AzureWebJobsStorage"                      				= "DefaultEndpointsProtocol=https;AccountName=mhpdcloudservicespenfunc;AccountKey=${module.retrieved_pensions_details_function.sa_primary_access_key};EndpointSuffix=core.windows.net"
    "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING" = "DefaultEndpointsProtocol=https;AccountName=pensionrequest${var.env};AccountKey=${module.pension_request_function.sa_primary_access_key};EndpointSuffix=core.windows.net"
  }
  pensions_retrieval_app_settings = {
    "PeiIntegrationServiceUrl"                                = "https://maps-api-management-${var.env}.azure-api.net/pei-integration-service/"
    "PeiRetryInterval"                                        = "5"
    "PeiRetryTimeout"                                         = "60"
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"                         = "true"
    "WEBSITE_RUN_FROM_PACKAGE"                                = "1"
    "WEBSITE_USE_PLACEHOLDER_DOTNETISOLATED"                  = "1"
    "APPLICATIONINSIGHTS_CONNECTION_STRING"                   = "InstrumentationKey=${module.pensions_retrieval_function.instrumentation_key};IngestionEndpoint=https://uksouth-1.in.applicationinsights.azure.com/;LiveEndpoint=https://uksouth.livediagnostics.monitor.azure.com/;ApplicationId=${module.pensions_retrieval_function.app_insights_app_id}"
    "PeiRetrievalDuration"                                    = "60"
    "PensionRetrievalServiceUrl"                              = "https://maps-api-management-${var.env}.azure-api.net/pension-retrieval-service/"
    "RetrievedPensionsServiceUrl"                             = "https://maps-api-management-${var.env}.azure-api.net/retrieved-pensions-record-service/"
    "PeiPollingInterval"                                      = "5"
    "CommonServiceBusConfiguration__InboundQueue"             = "pensions-retrieval-job"
    "CommonServiceBusConfiguration__OutboundQueue"            = "pension-details-request"
    "CosmosBusinessConfiguration__DatabaseId"                 = "mhpd-business-layer"
    "CosmosBusinessConfiguration__UserSessionDataContainer"   = "mhpdUserSessionData"
    "CosmosBusinessConfiguration__PensionsRetrievalContainer" = "mhpdPensionsRetrievalRecords"
  }

  retrieved_pensions_app_settings = {
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"                         = "true"
    "WEBSITE_RUN_FROM_PACKAGE"                                = "1"
    "WEBSITE_USE_PLACEHOLDER_DOTNETISOLATED"                  = "1"
    "APPLICATIONINSIGHTS_CONNECTION_STRING"                   = "InstrumentationKey=${module.retrieved_pensions_details_function.instrumentation_key};IngestionEndpoint=https://uksouth-1.in.applicationinsights.azure.com/;LiveEndpoint=https://uksouth.livediagnostics.monitor.azure.com/;ApplicationId=${module.retrieved_pensions_details_function.app_insights_app_id}"
    "CosmosBusinessConfiguration__DatabaseId"                 = "mhpd-business-layer"
    "CosmosBusinessConfiguration__RetrievedPensionsContainer" = "mhpdRetrievedPensionRecords"
    "CommonServiceBusConfiguration__InboundQueue"             = "retrieved-pension-details"
  }

  cryptopack_app_settings = {
    "StorageConnectionString"                  = "DefaultEndpointsProtocol=https;AccountName=cryptopackproc${var.env};AccountKey=${module.cryptopack_processor_function.sa_primary_access_key}"
    "KeyVaultSettings__KeyVaultUrl"            = "https://mhpd-${var.env}.vault.azure.net/"
    "KeyVaultSettings__TenantId"               = "bbe41032-8fce-4d42-bab5-44e21510886d"
    "KeyVaultSettings__ClientId"               = data.azurerm_key_vault_secret.user_client_id.value
    "KeyVaultSettings__ClientSecret"           = data.azurerm_key_vault_secret.user_client_secret.value
    "CryptopackSettings__MtlsCertificateName"  = "PdpMtls"
    "CryptopackSettings__PrivateKeySecretName" = "maps-cda-service-private-key"
    "CryptopackSettings__KidSecretName"        = "maps-cda-service-kid"
    "CryptopackSettings__PfxPassword"          = data.azurerm_key_vault_secret.pfx_password.value
    "WebAppSettings__AppName"                  = "CDA-service-${var.env}"
    "WebAppSettings__SubscriptionId"           = data.azurerm_client_config.current.subscription_id
    "WebAppSettings__ResourceGroupName"        = data.azurerm_resource_group.mhpd.name
    "WebAppSettings__JwtKeyVariable"           = "JwtSettings__PrivateKey"
    "WebAppSettings__JwtKidVariable"           = "JwtSettings__Kid"
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"          = "true"
    "WEBSITE_RUN_FROM_PACKAGE"                 = "1"
    "WEBSITE_USE_PLACEHOLDER_DOTNETISOLATED"   = "1"
    "APPLICATIONINSIGHTS_CONNECTION_STRING"    = "InstrumentationKey=${module.cryptopack_processor_function.instrumentation_key};IngestionEndpoint=https://uksouth-1.in.applicationinsights.azure.com/;LiveEndpoint=https://uksouth.livediagnostics.monitor.azure.com/;ApplicationId=${module.cryptopack_processor_function.app_insights_app_id}"
    "Manifest__KeyId"                          = "kid"
    "Manifest__MtlsCertificate"                = "certificate"
    "Manifest__MtlsChain"                      = "certificateChain"
    "Manifest__CertificatePair__PrivateKey"    = "certPrivateKey"
    "Manifest__CertificatePair__PublicKey"     = "certPublicKey"
    "Manifest__CertificatePair__AlgorithmType" = "1"
    "Manifest__JwtPair__PrivateKey"            = "jwtPrivateKey"
    "Manifest__JwtPair__PublicKey"             = "jwtPublicKey"
    "Manifest__JwtPair__AlgorithmType"         = "0"
  }
  cosmos_db_connection_string = {
    name  = "CosmosDBConnectionString"
    type  = "Custom"
    value = "AccountEndpoint=https://${var.product}-cosmos-${var.env}.documents.azure.com:443/;AccountKey=${data.azurerm_cosmosdb_account.this.primary_key}"
  }
  service_bus_connection_string = {
    name  = "ServiceBusConnectionString"
    type  = "Custom"
    value = data.azurerm_servicebus_namespace.this.default_primary_connection_string
  }
}