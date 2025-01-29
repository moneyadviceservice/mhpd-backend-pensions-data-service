module "token_integration_service" {
  source              = "github.com/moneyadviceservice/terraform-module-app-service.git"
  name                = "token-integration-service"
  product             = var.product
  resource_group_name = data.azurerm_resource_group.this.name
  location            = var.location
  env                 = var.env
  os_type             = "Linux"
  sku_name            = "B3"
  ftps_state          = "FtpsOnly"
  app_command_line    = "dotnet TokenIntegrationService.dll"
  dotnet_stack        = true
  app_settings = {
    "APPINSIGHTS_INSTRUMENTATIONKEY"                           = module.token_integration_service.instrumentation_key
    "APPINSIGHTS_PROFILERFEATURE_VERSION"                      = "1.0.0"
    "APPINSIGHTS_SNAPSHOTFEATURE_VERSION"                      = "1.0.0"
    "APPLICATIONINSIGHTS_CONNECTION_STRING"                    = "InstrumentationKey=${module.token_integration_service.instrumentation_key};IngestionEndpoint=https://uksouth-1.in.applicationinsights.azure.com/;LiveEndpoint=https://uksouth.livediagnostics.monitor.azure.com/;ApplicationId=${module.token_integration_service.app_insights_app_id}"
    "APPLICATIONINSIGHTS_ENABLESQLQUERYCOLLECTION"             = "disabled"
    "ApplicationInsightsAgent_EXTENSION_VERSION"               = "~2"
    "DISABLE_APPINSIGHTS_SDK"                                  = "disabled"
    "DiagnosticServices_EXTENSION_VERSION"                     = "~3"
    "IGNORE_APPINSIGHTS_SDK"                                   = "disabled"
    "InstrumentationEngine_EXTENSION_VERSION"                  = "disabled"
    "SnapshotDebugger_EXTENSION_VERSION"                       = "disabled"
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"                          = "true"
    "XDT_MicrosoftApplicationInsights_BaseExtensions"          = "disabled"
    "XDT_MicrosoftApplicationInsights_Mode"                    = "recommended"
    "XDT_MicrosoftApplicationInsights_PreemptSdk"              = "disabled"
    "CdaServiceUrl"                                            = "https://maps-api-management-${var.env}.azure-api.net/cda-integration-external/"
    "CosmosIntegrationConfiguration__DatabaseId"               = "mhpd-integration-layer"
    "CosmosIntegrationConfiguration__HolderNameCacheContainer" = "mhpdholderNameViewConfigurationData"
    "CosmosIntegrationConfiguration__JwkCacheContainer"        = "jwkUriEmulatorData"
    "CosmosDBConnectionString"                                 = "AccountEndpoint=https://${var.product}-cosmos-${var.env}.documents.azure.com:443/;AccountKey=${data.azurerm_cosmosdb_account.this.primary_key}"
  }
  tags = {}
}