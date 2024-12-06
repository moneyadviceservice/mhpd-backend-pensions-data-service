module "pdp_view_data_service_emulator" {
  source              = "github.com/moneyadviceservice/terraform-module-app-service.git"
  name                = "pdp-view-data-service-emulator"
  product             = var.product
  resource_group_name = data.azurerm_resource_group.this.name
  location            = var.location
  env                 = var.env
  os_type             = "Linux"
  sku_name            = "B3"
  ftps_state          = var.ftps_state
  app_command_line    = "dotnet PDPViewDataServicedEmulator.dll"
  dotnet_stack        = true
  app_settings = {
    "AzureCosmosDb__Account"                = "https://${var.product}-cosmos-${var.env}.documents.azure.com:443"
    "AzureCosmosDb__ContainerName"          = "viewdatapayloads"
    "AzureCosmosDb__DatabaseName"           = "testharness"
    "AzureCosmosDb__Key"                    = data.azurerm_cosmosdb_account.this.primary_key
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"       = true
    "APPLICATIONINSIGHTS_CONNECTION_STRING" = "InstrumentationKey=${module.pdp_view_data_service_emulator.instrumentation_key};IngestionEndpoint=https://uksouth-1.in.applicationinsights.azure.com/;LiveEndpoint=https://uksouth.livediagnostics.monitor.azure.com/;ApplicationId=${module.pdp_view_data_service_emulator.app_insights_app_id}"
  }
  tags = {}
}