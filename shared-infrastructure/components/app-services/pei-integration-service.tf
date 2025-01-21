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
    "DiagnosticServices_EXTENSION_VERSION"            = "~3"
    "IGNORE_APPINSIGHTS_SDK"                          = "disabled"
    "InstrumentationEngine_EXTENSION_VERSION"         = "disabled"
    "CdaServiceUrl"                                   = "https://maps-api-management-${var.env}.azure-api.net/cda-integration-external/"
    "MapsCdaServiceUrl"                               = "https://maps-api-management-${var.env}.azure-api.net/maps-cda-service/"
    "TokenIntegrationServiceUrl"                      = "https://maps-api-management-${var.env}.azure-api.net/token-integration-service/"
    "PensionRetrievalServiceUrl"                      = "https://maps-api-management-${var.env}.azure-api.net/pension-retrieval-service/"
    "RetrievedPensionsServiceUrl"                     = "https://maps-api-management-${var.env}.azure-api.net/retrieved-pensions-record-service/"
    "KeyVaultConfiguration__KeyVaultURL"              = "https://${var.product}-${var.env}.vault.azure.net/"
    "SnapshotDebugger_EXTENSION_VERSION"              = "disabled"
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"                 = "true"
    "XDT_MicrosoftApplicationInsights_BaseExtensions" = "disabled"
    "XDT_MicrosoftApplicationInsights_Mode"           = "recommended"
    "XDT_MicrosoftApplicationInsights_PreemptSdk"     = "disabled"
    "OutboundQueue"                                   = "pensions-retrieval-job"
    "PeiRetrievalDuration"                            = "60"
    "ServiceBusConnectionString"                      = "Endpoint=sb://mhpd-sbns-${var.env}.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=${data.azurerm_servicebus_namespace.this.default_primary_key}"
    "ViewDataRetrievalDuration"                       = "5"

  }
  tags = {}
}