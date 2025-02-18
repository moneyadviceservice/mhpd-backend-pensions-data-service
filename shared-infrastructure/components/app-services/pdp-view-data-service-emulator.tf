module "pdp_view_data_service_emulator" {
  source              = "github.com/moneyadviceservice/terraform-module-app-service.git?ref=vnet_integration"
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
    value = "AccountEndpoint=https://${var.product}-cosmos-testharness.documents.azure.com:443/;AccountKey=${data.azurerm_cosmosdb_account.testharness.primary_key}"
  }]
  app_settings = {
    "JwtSettings__ExpiryInSeconds"                           = "600"
    "JwtSettings__PrivateKey"                                = data.azurerm_key_vault_secret.cda_service_emulator_private_key.value
    "JwtSettings__Audience"                                  = "https://pdp/ig/token"
    "JwtSettings__Issuer"                                    = "https://emulators.maps.org.uk/am/oauth2"
    "JwtSettings__Kid"                                       = data.azurerm_key_vault_secret.jwt_settings_kid.value
    "JwtSettings__Subject"                                   = data.azurerm_key_vault_secret.jwt_settings_subject.value
    "MhpdCosmosConfiguration__DatabaseName"                  = "mhpd-testharness"
    "MhpdCosmosConfiguration__ViewdatapayloadsContainerName" = "viewdatapayloads"
    "CosmosDBConnectionString"                               = "AccountEndpoint=https://${var.product}-cosmos-${var.env}.documents.azure.com:443/;AccountKey=${data.azurerm_cosmosdb_account.this.primary_key}"
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"                        = true
    "APPLICATIONINSIGHTS_CONNECTION_STRING"                  = "InstrumentationKey=${module.pdp_view_data_service_emulator.instrumentation_key};IngestionEndpoint=https://uksouth-1.in.applicationinsights.azure.com/;LiveEndpoint=https://uksouth.livediagnostics.monitor.azure.com/;ApplicationId=${module.pdp_view_data_service_emulator.app_insights_app_id}"
    "Mtls__ClientCertificate"                                = "PdpMtls"
    "Mtls__EnforceClientCertificate"                         = "False"
    "Mtls__VaultUri"                                         = "https://${var.product}-${var.env}.vault.azure.net/"
  }
  tags = {}
}

resource "azurerm_key_vault_access_policy" "pdp_view_data_service_application_access" {
  key_vault_id = data.azurerm_key_vault.mhpd.id

  object_id = module.pdp_view_data_service_emulator.system_assigned_identity_object_id
  tenant_id = data.azurerm_client_config.current.tenant_id

  key_permissions = [
    "List",
    "Get",
  ]

  secret_permissions = [
    "List",
    "Get",
  ]
  certificate_permissions = [
    "List",
    "Get",
  ]
}