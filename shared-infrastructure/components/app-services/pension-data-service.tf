module "pension_data_service" {
  source              = "github.com/moneyadviceservice/terraform-module-app-service.git"
  name                = "pension-data-service"
  product             = var.product
  resource_group_name = data.azurerm_resource_group.this.name
  location            = var.location
  env                 = var.env
  os_type             = "Linux"
  sku_name            = "B3"
  ftps_state          = var.ftps_state
  app_command_line    = "dotnet PensionsDataService.dll"
  dotnet_stack        = true
  app_settings = {
    "CommonServiceBusConfiguration__OutboundQueue" = "pensions-retrieval-job"
    "OutboundQueue"                                = "pensions-retrieval-job"
    "ServiceBusConnectionString"                   = data.azurerm_servicebus_namespace.this.default_primary_connection_string
    "PensionRetrievalServiceUrl"                   = "https://maps-api-management-${var.env}.azure-api.net/pension-retrieval-service/"
    "RetrievedPensionsServiceUrl"                  = "https://maps-api-management-${var.env}.azure-api.net/retrieved-pensions-record-service/"
    "tokenIntegrationServiceUrl"                   = "https://maps-api-management-${var.env}.azure-api.net/token-integration-service/"
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"              = true
    "APPLICATIONINSIGHTS_CONNECTION_STRING"        = "InstrumentationKey=${module.pension_data_service.instrumentation_key};IngestionEndpoint=https://uksouth-1.in.applicationinsights.azure.com/;LiveEndpoint=https://uksouth.livediagnostics.monitor.azure.com/;ApplicationId=${module.pension_data_service.app_insights_app_id}"
  }
  tags = {}
}