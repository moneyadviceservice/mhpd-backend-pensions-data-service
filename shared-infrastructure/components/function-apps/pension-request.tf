module "pension_request_function" {
  source = "github.com/moneyadviceservice/terraform-module-function-app.git?ref=main"

  os_type             = "Windows"
  product             = var.product
  create_service_plan = true
  resource_group_name = data.azurerm_resource_group.mhpd.name
  name                = "pension-request"
  location            = var.location
  env                 = var.env
  sku_name            = "EP1"

  dotnet_stack = true

  app_settings = {
    "APPLICATIONINSIGHTS_CONNECTION_STRING"                    = "InstrumentationKey=${module.pension_request_function.instrumentation_key};IngestionEndpoint=https://uksouth-1.in.applicationinsights.azure.com/;LiveEndpoint=https://uksouth.livediagnostics.monitor.azure.com/;ApplicationId=${module.pension_request_function.app_insights_app_id}"
    "CdaServiceUrl"                                            = "https://maps-api-management-${var.env}.azure-api.net/cda-integration-external/"
    "PdpServiceUrl"                                            = "https://maps-api-management-${var.env}.azure-api.net/view-data-external/"
    "TokenIntegrationServiceUrl"                               = "https://maps-api-management-${var.env}.azure-api.net/token-integration-service/"
    "MapsCdaServiceUrl"                                        = "https://maps-api-management-${var.env}.azure-api.net/maps-cda-service/"
    "ContainerId"                                              = "${var.product}holderNameViewConfigurationData"
    "ContainerPartitionKey"                                    = "/holdername_guid"
    "DatabaseId"                                               = "${var.product}-integration-layer"
    "CosmosDBConnectionString"                                 = "AccountEndpoint=https://${var.product}-cosmos-${var.env}.documents.azure.com:443/;AccountKey=${data.azurerm_cosmosdb_account.this.primary_key}"
    "FUNCTIONS_EXTENSION_VERSION"                              = "~4"
    "FUNCTIONS_WORKER_RUNTIME"                                 = "dotnet-isolated"
    "InboundQueue"                                             = "pension-details-request"
    "OutboundQueue"                                            = "retrieved-pension-details"
    "ServiceBusConnectionString"                               = data.azurerm_servicebus_namespace.this.default_primary_connection_string
    "WEBSITE_CONTENTSHARE"                                     = "pensionrequestfunctionaee5"
    "WEBSITE_RUN_FROM_PACKAGE"                                 = "1"
    "WEBSITE_USE_PLACEHOLDER_DOTNETISOLATED"                   = "1"
    "CosmosIntegrationConfiguration__DatabaseId"               = "mhpd-integration-layer"
    "CosmosIntegrationConfiguration__HolderNameCacheContainer" = "mhpdholderNameViewConfigurationData"
    "CosmosIntegrationConfiguration__JwkCacheContainer"        = "jwkUriEmulatorData"
    # "AzureWebJobsStorage"                      				= "DefaultEndpointsProtocol=https;AccountName=mhpdcloudservicespenfunc;AccountKey=${module.retrieved_pensions_details_function.sa_primary_access_key};EndpointSuffix=core.windows.net"
    # "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING" 				= "DefaultEndpointsProtocol=https;AccountName=mhpdcloudservicespenfunc;AccountKey=${module.retrieved_pensions_details_function.sa_primary_access_key};EndpointSuffix=core.windows.net"
  }
}