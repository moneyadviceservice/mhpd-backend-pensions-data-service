module "retrieved_pensions_details_function" {
  source = "github.com/moneyadviceservice/terraform-module-function-app.git?ref=main"

  os_type             = "Windows"
  product             = var.product
  create_service_plan = true
  resource_group_name = data.azurerm_resource_group.mhpd.name
  name                = "retrieved-pensions"
  location            = var.location
  env                 = var.env
  sku_name            = "EP1"

  dotnet_stack = true

  app_settings = {
    "ContainerId"                            = "${var.product}RetrievedPensionRecords"
    "ContainerPartitionKey"                  = "/pensionsRetrievalRecordId"
    "DatabaseId"                             = "${var.product}-business-layer"
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"        = "true"
    "WEBSITE_RUN_FROM_PACKAGE"               = "1"
    "WEBSITE_USE_PLACEHOLDER_DOTNETISOLATED" = "1"
    "InboundQueue"                           = "retrieved-pension-details"
    "ServiceBusConnectionstring"             = data.azurerm_servicebus_namespace.this.default_primary_connection_string
    "CosmosDBConnectionString"               = "AccountEndpoint=https://${var.product}-cosmos-${var.env}.documents.azure.com:443/;AccountKey=${data.azurerm_cosmosdb_account.this.primary_key}"
    "APPLICATIONINSIGHTS_CONNECTION_STRING"  = "InstrumentationKey=${module.retrieved_pensions_details_function.instrumentation_key};IngestionEndpoint=https://uksouth-1.in.applicationinsights.azure.com/;LiveEndpoint=https://uksouth.livediagnostics.monitor.azure.com/;ApplicationId=${module.retrieved_pensions_details_function.app_insights_app_id}"
    "ViewDataRetrievalDuration"              = "5"
  }
}