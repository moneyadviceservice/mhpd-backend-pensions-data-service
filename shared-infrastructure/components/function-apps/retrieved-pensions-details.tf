module "retrieved_pensions_details_function" {
  source = "github.com/moneyadviceservice/terraform-module-function-app.git?ref=vnet_integration"

  os_type                 = "Windows"
  product                 = var.product
  create_service_plan     = true
  resource_group_name     = data.azurerm_resource_group.mhpd.name
  name                    = "retrieved-pensions"
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
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"                         = "true"
    "WEBSITE_RUN_FROM_PACKAGE"                                = "1"
    "WEBSITE_USE_PLACEHOLDER_DOTNETISOLATED"                  = "1"
    "APPLICATIONINSIGHTS_CONNECTION_STRING"                   = "InstrumentationKey=${module.retrieved_pensions_details_function.instrumentation_key};IngestionEndpoint=https://uksouth-1.in.applicationinsights.azure.com/;LiveEndpoint=https://uksouth.livediagnostics.monitor.azure.com/;ApplicationId=${module.retrieved_pensions_details_function.app_insights_app_id}"
    "CosmosBusinessConfiguration__DatabaseId"                 = "mhpd-business-layer"
    "CosmosBusinessConfiguration__RetrievedPensionsContainer" = "mhpdRetrievedPensionRecords"
    "CommonServiceBusConfiguration__InboundQueue"             = "retrieved-pension-details"
  }
}