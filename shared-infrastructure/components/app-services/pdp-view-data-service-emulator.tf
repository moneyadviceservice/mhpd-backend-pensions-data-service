module "pdp_view_data_service_emulator" {
  source              = "github.com/moneyadviceservice/terraform-module-app-service.git?ref=add_connection_string"
  name                = "pdp-view-data-service-emulator"
  product             = var.product
  resource_group_name = data.azurerm_resource_group.this.name
  location            = var.location
  env                 = var.env
  os_type             = "Linux"
  sku_name            = "B3"
  ftps_state          = var.ftps_state
  app_command_line    = "dotnet PDPViewDataServiceEmulator.dll"
  dotnet_stack        = true
  connection_strings = [{
    name  = "CosmosDBConnectionString"
    type  = "Custom"
    value = "AccountEndpoint=https://${var.product}-cosmos-${var.env}.documents.azure.com:443/;AccountKey=${data.azurerm_cosmosdb_account.this.primary_key}"
  }]
  app_settings = {
    "JwtSettings__ExpiryInSeconds"                           = "600"
    "JwtSettings__PrivateKey"                                = data.azurerm_key_vault_secret.CDA_service_emulator_private_key.value
    "JwtSettings__Audience"                                  = "https://pdp/ig/token"
    "JwtSettings__Issuer"                                    = "https://emulators.maps.org.uk/am/oauth2"
    "JwtSettings__Kid"                                       = data.azurerm_key_vault_secret.jwt_settings_kid.value
    "JwtSettings__Subject"                                   = data.azurerm_key_vault_secret.jwt_settings_subject.value
    "MhpdCosmosConfiguration__DatabaseName"                  = "mhpd-testharness"
    "MhpdCosmosConfiguration__ViewdatapayloadsContainerName" = "viewdatapayloads"
    "CosmosDBConnectionString"                               = "AccountEndpoint=https://${var.product}-cosmos-${var.env}.documents.azure.com:443/;AccountKey=${data.azurerm_cosmosdb_account.this.primary_key}"
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"                        = true
    "APPLICATIONINSIGHTS_CONNECTION_STRING"                  = "InstrumentationKey=${module.pdp_view_data_service_emulator.instrumentation_key};IngestionEndpoint=https://uksouth-1.in.applicationinsights.azure.com/;LiveEndpoint=https://uksouth.livediagnostics.monitor.azure.com/;ApplicationId=${module.pdp_view_data_service_emulator.app_insights_app_id}"
  }
  tags = {}
}