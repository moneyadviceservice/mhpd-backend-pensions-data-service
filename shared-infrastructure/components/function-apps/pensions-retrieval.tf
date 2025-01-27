module "pensions_retrieval_function" {
  source = "github.com/moneyadviceservice/terraform-module-function-app.git?ref=main"

  os_type             = "Windows"
  product             = var.product
  create_service_plan = true
  resource_group_name = data.azurerm_resource_group.mhpd.name
  name                = "pensions-retrieval"
  location            = var.location
  env                 = var.env
  sku_name            = "EP1"
  dotnet_stack        = true

  app_settings = {
    "ContainerId"                            = "${var.product}PensionsRetrievalRecords"
    "ContainerPartitionKey"                  = "/userSessionId"
    "CosmosDBConnectionString"               = "AccountEndpoint=https://${var.product}-cosmos-${var.env}.documents.azure.com:443/;AccountKey=${data.azurerm_cosmosdb_account.this.primary_key}"
    "DatabaseId"                             = "${var.product}-business-layer"
    "InboundQueue"                           = "pensions-retrieval-job"
    "OutboundQueue"                          = "pension-details-request"
    "PeiIntegrationServiceUrl"               = "https://maps-api-management-${var.env}.azure-api.net/pei-integration-service/"
    "PeiRetryInterval"                       = "5"
    "PeiRetryTimeout"                        = "60"
    "ServiceBusConnectionstring"             = data.azurerm_servicebus_namespace.this.default_primary_connection_string
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"        = "true"
    "WEBSITE_RUN_FROM_PACKAGE"               = "1"
    "WEBSITE_USE_PLACEHOLDER_DOTNETISOLATED" = "1"
    "APPLICATIONINSIGHTS_CONNECTION_STRING"  = "InstrumentationKey=${module.pensions_retrieval_function.instrumentation_key};IngestionEndpoint=https://uksouth-1.in.applicationinsights.azure.com/;LiveEndpoint=https://uksouth.livediagnostics.monitor.azure.com/;ApplicationId=${module.pensions_retrieval_function.app_insights_app_id}"
  }
}
