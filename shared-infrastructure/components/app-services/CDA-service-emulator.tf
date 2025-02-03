module "CDA_service_emulator" {
  source                 = "github.com/moneyadviceservice/terraform-module-app-service.git?ref=add_connection_string"
  name                   = "CDA-service-emulator"
  product                = var.product
  resource_group_name    = data.azurerm_resource_group.this.name
  location               = var.location
  env                    = var.env
  os_type                = "Linux"
  sku_name               = "B3"
  ftps_state             = var.ftps_state
  app_command_line       = "dotnet CDAServiceEmulator.dll"
  dotnet_stack           = true
  enable_client_affinity = true
  connection_strings = [{
    name  = "CosmosDBConnectionString"
    type  = "Custom"
    value = "AccountEndpoint=https://${var.product}-cosmos-${var.env}.documents.azure.com:443/;AccountKey=${data.azurerm_cosmosdb_account.this.primary_key}"
  }]
  app_settings = {
    "APPINSIGHTS_INSTRUMENTATIONKEY"                                          = module.CDA_service.instrumentation_key
    "APPINSIGHTS_PROFILERFEATURE_VERSION"                                     = "1.0.0"
    "APPINSIGHTS_SNAPSHOTFEATURE_VERSION"                                     = "1.0.0"
    "APPLICATIONINSIGHTS_CONNECTION_STRING"                                   = "InstrumentationKey=${module.CDA_service_emulator.instrumentation_key};IngestionEndpoint=https://uksouth-1.in.applicationinsights.azure.com/;LiveEndpoint=https://uksouth.livediagnostics.monitor.azure.com/;ApplicationId=${module.CDA_service_emulator.app_insights_app_id}"
    "APPLICATIONINSIGHTS_ENABLESQLQUERYCOLLECTION"                            = "disabled"
    "ApplicationInsightsAgent_EXTENSION_VERSION"                              = "~2"
    "DISABLE_APPINSIGHTS_SDK"                                                 = "disabled"
    "DiagnosticServices_EXTENSION_VERSION"                                    = "~3"
    "IGNORE_APPINSIGHTS_SDK"                                                  = "disabled"
    "InstrumentationEngine_EXTENSION_VERSION"                                 = "disabled"
    "CosmosDBConnectionString"                                                = "AccountEndpoint=https://${var.product}-cosmos-${var.env}.documents.azure.com:443/;AccountKey=${data.azurerm_cosmosdb_account.this.primary_key}"
    "SnapshotDebugger_EXTENSION_VERSION"                                      = "disabled"
    "MhpdCosmosConfiguration__CdaPeisEmulatorScenarioModelContainerName"      = "cdaPeisEmulatorScenarioModels"
    "MhpdCosmosConfiguration__CdaPeisEmulatorTestInstanceDataContainerName"   = "cdaPeisEmulatorTestInstanceData"
    "MhpdCosmosConfiguration__HolderNameConfigurationModelsContainerName"     = "holdernameViewConfigurationEmulatorData"
    "MhpdCosmosConfiguration__TokenEmulatorPiesIdScenarioModelsContainerName" = "tokenEmulatorPiesIdScenarioModels"
    "MhpdCosmosConfiguration__DatabaseName"                                   = "mhpd-testharness"
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"                                         = "true"
    "XDT_MicrosoftApplicationInsights_BaseExtensions"                         = "disabled"
    "XDT_MicrosoftApplicationInsights_Mode"                                   = "recommended"
    "XDT_MicrosoftApplicationInsights_PreemptSdk"                             = "disabled"
    "JwtSettings__ExpiryInSeconds"                                            = "600"
    "JwtSettings__PrivateKey"                                                 = data.azurerm_key_vault_secret.cda_service_emulator_private_key.value
    "JwtSettings__Audience"                                                   = "https://pdp/ig/token"
    "JwtSettings__Issuer"                                                     = "https://emulators.maps.org.uk/am/oauth2"
    "JwtSettings__Kid"                                                        = data.azurerm_key_vault_secret.jwt_settings_kid.value
    "JwtSettings__Subject"                                                    = data.azurerm_key_vault_secret.jwt_settings_subject.value
    "Mtls__ClientCertificate"                                                 = "PdpMtls"
    "Mtls__EnforceClientCertificate"                                          = "False"
    "Mtls__VaultUri"                                                          = "https://${var.product}-${var.env}.vault.azure.net/"
  }
  tags = {}
}

resource "azurerm_key_vault_access_policy" "cda_emulator_application_access" {
  key_vault_id = data.azurerm_key_vault.mhpd.id

  object_id = module.CDA_service_emulator.system_assigned_identity_object_id
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

# access for old emular - to be removed during decomissioning
resource "azurerm_key_vault_access_policy" "cda_old_emulator_application_access" {
  key_vault_id = data.azurerm_key_vault.mhpd.id

  object_id = "81894e6a-9e4f-4107-8ae4-1c67ee3c1f46"
  tenant_id = data.azurerm_client_config.current.tenant_id

  key_permissions = [
    "List",
    "Get",
  ]

  secret_permissions = [
    "List",
    "Get",
  ]
}