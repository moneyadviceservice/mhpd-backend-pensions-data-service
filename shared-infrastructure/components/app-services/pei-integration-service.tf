module "pei_integration_service" {
  source              = "github.com/moneyadviceservice/terraform-module-app-service.git"
  name                = "pei-integration-service"
  product             = var.product
  resource_group_name = data.azurerm_resource_group.this.name
  location            = var.location
  env                 = var.env
  os_type             = "Linux"
  sku_name            = "B3"
  ftps_state          = var.ftps_state
  app_command_line    = "dotnet PeiIntegrationService.dll"
  dotnet_stack        = true
  app_settings = {
    "APPINSIGHTS_INSTRUMENTATIONKEY"                  = module.pei_integration_service.instrumentation_key
    "APPINSIGHTS_PROFILERFEATURE_VERSION"             = "1.0.0"
    "APPINSIGHTS_SNAPSHOTFEATURE_VERSION"             = "1.0.0"
    "APPLICATIONINSIGHTS_CONNECTION_STRING"           = "InstrumentationKey=${module.pei_integration_service.instrumentation_key};IngestionEndpoint=https://uksouth-1.in.applicationinsights.azure.com/;LiveEndpoint=https://uksouth.livediagnostics.monitor.azure.com/;ApplicationId=${module.pei_integration_service.app_insights_app_id}"
    "APPLICATIONINSIGHTS_ENABLESQLQUERYCOLLECTION"    = "disabled"
    "ApplicationInsightsAgent_EXTENSION_VERSION"      = "~2"
    "DISABLE_APPINSIGHTS_SDK"                         = "disabled"
    "CdaServiceUrl"                                   = "https://maps-apim-${var.env}.azure-api.net/cda-integration/"
    "DiagnosticServices_EXTENSION_VERSION"            = "~3"
    "IGNORE_APPINSIGHTS_SDK"                          = "disabled"
    "InstrumentationEngine_EXTENSION_VERSION"         = "disabled"
    "MapsCdaServiceUrl"                               = "https://maps-apim-${var.env}.azure-api.net/mhpd/"
    "TokenIntegrationServiceUrl"                      = "https://maps-apim-${var.env}.azure-api.net/mhpd/"
    "KeyVaultConfiguration__KeyVaultURL"              = "https://${var.product}-${var.env}.vault.azure.net/"
    "SnapshotDebugger_EXTENSION_VERSION"              = "disabled"
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"                 = "true"
    "XDT_MicrosoftApplicationInsights_BaseExtensions" = "disabled"
    "XDT_MicrosoftApplicationInsights_Mode"           = "recommended"
    "XDT_MicrosoftApplicationInsights_PreemptSdk"     = "disabled"
  }
  tags = {}
}