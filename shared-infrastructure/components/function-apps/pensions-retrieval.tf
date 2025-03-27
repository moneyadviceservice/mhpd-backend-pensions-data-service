module "pensions_retrieval_function" {
  source = "github.com/moneyadviceservice/terraform-module-function-app.git?ref=vnet_integration"

  os_type                 = "Windows"
  product                 = var.product
  create_service_plan     = true
  resource_group_name     = data.azurerm_resource_group.mhpd.name
  name                    = "pensions-retrieval"
  location                = var.location
  env                     = var.env
  sku_name                = "EP1"
  dotnet_stack            = true
  enable_vnet_integration = local.enable_vnet_integration
  subnet_id               = local.subnet_id
  connection_strings = [
    {
      name  = "CosmosDBConnectionString"
      type  = "Custom"
      value = "AccountEndpoint=https://${var.product}-cosmos-${var.env}.documents.azure.com:443/;AccountKey=${data.azurerm_cosmosdb_account.this.primary_key}"
    },
    {
      name  = "ServiceBusConnectionString"
      type  = "Custom"
      value = data.azurerm_servicebus_namespace.this.default_primary_connection_string
    }
  ]

  app_settings = {
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
}
