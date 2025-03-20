module "CDA_service" {
  source                 = "github.com/moneyadviceservice/terraform-module-app-service.git?ref=vnet_integration"
  name                   = "CDA-service"
  product                = var.product
  resource_group_name    = data.azurerm_resource_group.this.name
  location               = var.location
  env                    = var.env
  os_type                = "Linux"
  sku_name               = "B3"
  ftps_state             = var.ftps_state
  app_command_line       = "dotnet MaPSCDAService.dll"
  dotnet_stack           = true
  enable_client_affinity = true

  enable_vnet_integration = local.enable_vnet_integration
  subnet_id               = local.subnet_id

  app_settings = {
    "APPINSIGHTS_INSTRUMENTATIONKEY"                        = module.CDA_service.instrumentation_key
    "APPINSIGHTS_PROFILERFEATURE_VERSION"                   = "1.0.0"
    "APPINSIGHTS_SNAPSHOTFEATURE_VERSION"                   = "1.0.0"
    "APPLICATIONINSIGHTS_CONNECTION_STRING"                 = "InstrumentationKey=${module.CDA_service.instrumentation_key};IngestionEndpoint=https://uksouth-1.in.applicationinsights.azure.com/;LiveEndpoint=https://uksouth.livediagnostics.monitor.azure.com/;ApplicationId=${module.CDA_service.app_insights_app_id}"
    "APPLICATIONINSIGHTS_ENABLESQLQUERYCOLLECTION"          = "disabled"
    "ApplicationInsightsAgent_EXTENSION_VERSION"            = "~2"
    "DISABLE_APPINSIGHTS_SDK"                               = "disabled"
    "DiagnosticServices_EXTENSION_VERSION"                  = "~3"
    "IGNORE_APPINSIGHTS_SDK"                                = "disabled"
    "InstrumentationEngine_EXTENSION_VERSION"               = "disabled"
    "KeyVaultConfiguration__KeyVaultURL"                    = "https://${var.product}-${var.env}.vault.azure.net/"
    "SnapshotDebugger_EXTENSION_VERSION"                    = "disabled"
    "UriSettings__RedirectTargetUrl"                        = var.env == "staging" ? "${var.cda_base_url}/ig/authorize" : "https://pdp-data-access-test-harness.netlify.app/"
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"                       = "true"
    "XDT_MicrosoftApplicationInsights_BaseExtensions"       = "disabled"
    "XDT_MicrosoftApplicationInsights_Mode"                 = "recommended"
    "XDT_MicrosoftApplicationInsights_PreemptSdk"           = "disabled"
    "JwtSettings__PrivateKey"                               = data.azurerm_key_vault_secret.maps_cda_service_private_key.value
    "JwtSettings__ExpiryInSeconds"                          = 600
    "JwtSettings__Audience"                                 = var.cda_base_url
    "JwtSettings__Kid"                                      = data.azurerm_key_vault_secret.maps_cda_service_kid.value
    "JwtSettings__Role"                                     = "owner"
    "TokenIntegrationServiceUrl"                            = "https://maps-api-management-${var.env}.azure-api.net/token-integration-service/"
    "PeiIntegrationServiceUrl"                              = "https://maps-api-management-${var.env}.azure-api.net/pei-integration-service/"
    "CosmosBusinessConfiguration__DatabaseId"               = "mhpd-business-layer"
    "CosmosBusinessConfiguration__UserSessionDataContainer" = "mhpdUserSessionData"
    "CosmosDBConnectionString"                              = "AccountEndpoint=https://${var.product}-cosmos-${var.env}.documents.azure.com:443/;AccountKey=${data.azurerm_cosmosdb_account.this.primary_key}"
  }
  tags = {}
}

resource "azurerm_key_vault_access_policy" "cda_application_access" {
  key_vault_id = data.azurerm_key_vault.mhpd.id

  object_id = module.CDA_service.system_assigned_identity_object_id
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
