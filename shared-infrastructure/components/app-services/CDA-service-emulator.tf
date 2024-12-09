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
    "KeyVaultConfiguration__KeyVaultURL"                                      = "https://${var.product}-${var.env}.vault.azure.net/"
    "SnapshotDebugger_EXTENSION_VERSION"                                      = "disabled"
    "MhpdCosmosConfiguration__CdaPeisEmulatorScenarioModelContainerName"      = "cdaPeisEmulatorScenarioModels"
    "MhpdCosmosConfiguration__CdaPeisEmulatorTestInstanceDataContainerName"   = "cdaPeisEmulatorTestInstanceData"
    "MhpdCosmosConfiguration__HolderNameConfigurationModelsContainerName"     = "holdernameViewConfigurationEmulatorData"
    "MhpdCosmosConfiguration__TokenEmulatorPiesIdScenarioModelsContainerName" = "tokenEmulatorPiesIdScenarioModels"
    # "WEBSITE_HTTPLOGGING_RETENTION_DAYS"                                      = "30"
    "MhpdCosmosConfiguration__DatabaseName"           = "mhpd-testharness"
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"                 = "true"
    "XDT_MicrosoftApplicationInsights_BaseExtensions" = "disabled"
    "XDT_MicrosoftApplicationInsights_Mode"           = "recommended"
    "XDT_MicrosoftApplicationInsights_PreemptSdk"     = "disabled"
    "JwtSettings__ExpiryInSeconds"                    = "600"
    "JwtSettings__PrivateKey"                         = data.azurerm_key_vault_secret.CDA_service_emulator_private_key.value
    "JwtSettings__Audience"                           = "https://pdp/ig/token"
    "JwtSettings__Issuer"                             = "https://emulators.maps.org.uk/am/oauth2"
    "JwtSettings__Kid"                                = data.azurerm_key_vault_secret.jwt_settings_kid.value
    "JwtSettings__Subject"                            = data.azurerm_key_vault_secret.jwt_settings_subject.value
  }
  tags = {}
}