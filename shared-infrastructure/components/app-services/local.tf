locals {
  enable_vnet_integration       = var.env == "staging" || var.env == "prod" ? true : false
  subnet_id                     = length(data.azurerm_subnet.mhpd-subnet) > 0 ? data.azurerm_subnet.mhpd-subnet["exists"].id : null
  public_network_access_enabled = var.env == "staging" || var.env == "prod" ? false : true
  sku_name                      = var.env == "staging" || var.env == "prod" ? "P1v2" : "B3"
  cda_asp_name                  = var.env == "staging" || var.env == "prod" ? "cda-service-asp-${var.env}" : "${var.product}-asp-${var.env}"
  pei_asp_name                  = var.env == "staging" || var.env == "prod" ? "pei-integration-asp-${var.env}" : "${var.product}-asp-${var.env}"
  pension_data_asp_name         = var.env == "staging" || var.env == "prod" ? "Pension-data-asp-${var.env}" : "${var.product}-asp-${var.env}"
  token_integration_asp_name    = var.env == "staging" || var.env == "prod" ? "token-integration-asp-${var.env}" : "${var.product}-asp-${var.env}"
  create_service_plan           = var.env == "staging" || var.env == "prod" ? true : false
  zone_redundant                = var.env == "prod" ? true : false

  cda_emulator_app_settings = {
    "APPINSIGHTS_INSTRUMENTATIONKEY"                                                 = module.CDA_service.instrumentation_key
    "APPINSIGHTS_PROFILERFEATURE_VERSION"                                            = "1.0.0"
    "APPINSIGHTS_SNAPSHOTFEATURE_VERSION"                                            = "1.0.0"
    "APPLICATIONINSIGHTS_CONNECTION_STRING"                                          = "InstrumentationKey=${module.CDA_service_emulator.instrumentation_key};IngestionEndpoint=https://uksouth-1.in.applicationinsights.azure.com/;LiveEndpoint=https://uksouth.livediagnostics.monitor.azure.com/;ApplicationId=${module.CDA_service_emulator.app_insights_app_id}"
    "APPLICATIONINSIGHTS_ENABLESQLQUERYCOLLECTION"                                   = "disabled"
    "ApplicationInsightsAgent_EXTENSION_VERSION"                                     = "~2"
    "DISABLE_APPINSIGHTS_SDK"                                                        = "disabled"
    "DiagnosticServices_EXTENSION_VERSION"                                           = "~3"
    "IGNORE_APPINSIGHTS_SDK"                                                         = "disabled"
    "InstrumentationEngine_EXTENSION_VERSION"                                        = "disabled"
    "SnapshotDebugger_EXTENSION_VERSION"                                             = "disabled"
    "CosmosTestHarnessConfiguration__CdaPeisEmulatorScenarioModelContainerName"      = "cdaPeisEmulatorScenarioModels"
    "CosmosTestHarnessConfiguration__CdaPeisEmulatorTestInstanceDataContainerName"   = "cdaPeisEmulatorTestInstanceData"
    "CosmosTestHarnessConfiguration__HolderNameConfigurationModelsContainerName"     = "holdernameViewConfigurationEmulatorData"
    "CosmosTestHarnessConfiguration__TokenEmulatorPiesIdScenarioModelsContainerName" = "tokenEmulatorPiesIdScenarioModels"
    "CosmosTestHarnessConfiguration__ViewDataModelContainerName"                     = "viewdatapayloads"
    "CosmosTestHarnessConfiguration__DatabaseName"                                   = "mhpd-testharness-${var.env}"
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"                                                = "true"
    "XDT_MicrosoftApplicationInsights_BaseExtensions"                                = "disabled"
    "XDT_MicrosoftApplicationInsights_Mode"                                          = "recommended"
    "XDT_MicrosoftApplicationInsights_PreemptSdk"                                    = "disabled"
    "JwtSettings__ExpiryInSeconds"                                                   = "600"
    "JwtSettings__PrivateKey"                                                        = data.azurerm_key_vault_secret.cda_service_emulator_private_key.value
    "JwtSettings__Audience"                                                          = "https://pdp/ig/token"
    "JwtSettings__Issuer"                                                            = "https://emulators.maps.org.uk/am/oauth2"
    "JwtSettings__Kid"                                                               = data.azurerm_key_vault_secret.jwt_settings_kid.value
    "JwtSettings__Subject"                                                           = data.azurerm_key_vault_secret.jwt_settings_subject.value
    "Mtls__ClientCertificate"                                                        = "PdpMtls"
    "Mtls__EnforceClientCertificate"                                                 = "False"
    "Mtls__VaultUri"                                                                 = "https://${var.product}-${var.env}.vault.azure.net/"
    "CdaServiceUrl"                                                                  = "https://maps-api-management-${var.env}.azure-api.net/cda-integration-external/"
  }

  cda_service_app_settings = {
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
    "UriSettings__RedirectTargetUrl"                        = var.env == "staging" || var.env == "prod" ? "${var.cda_base_url}/ig/authorize" : var.env == "test" ? "https://sys.pensionsdashboards.org.uk/ig/authorize" : "https://pdp-data-access-test-harness.netlify.app/"
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
  }
  pdp_view_data_service_emulator_app_settings = {
    "JwtSettings__ExpiryInSeconds"                               = "600"
    "JwtSettings__PrivateKey"                                    = data.azurerm_key_vault_secret.cda_service_emulator_private_key.value
    "JwtSettings__Audience"                                      = "https://pdp/ig/token"
    "JwtSettings__Issuer"                                        = "https://emulators.maps.org.uk/am/oauth2"
    "JwtSettings__Kid"                                           = data.azurerm_key_vault_secret.jwt_settings_kid.value
    "JwtSettings__Subject"                                       = data.azurerm_key_vault_secret.jwt_settings_subject.value
    "CosmosTestHarnessConfiguration__ViewDataModelContainerName" = "viewdatapayloads"
    "CosmosTestHarnessConfiguration__DatabaseName"               = "mhpd-testharness-${var.env}"
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"                            = true
    "APPLICATIONINSIGHTS_CONNECTION_STRING"                      = "InstrumentationKey=${module.pdp_view_data_service_emulator.instrumentation_key};IngestionEndpoint=https://uksouth-1.in.applicationinsights.azure.com/;LiveEndpoint=https://uksouth.livediagnostics.monitor.azure.com/;ApplicationId=${module.pdp_view_data_service_emulator.app_insights_app_id}"
    "Mtls__ClientCertificate"                                    = "PdpMtls"
    "Mtls__EnforceClientCertificate"                             = "False"
    "Mtls__VaultUri"                                             = "https://${var.product}-${var.env}.vault.azure.net/"
    "CdaServiceUrl"                                              = "https://maps-api-management-${var.env}.azure-api.net/cda-integration-external/"
  }
  pei_integration_app_settings = {
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
    "KeyVaultConfiguration__KeyVaultURL"              = "https://${var.product}-${var.env}.vault.azure.net/"
    "SnapshotDebugger_EXTENSION_VERSION"              = "disabled"
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"                 = "true"
    "XDT_MicrosoftApplicationInsights_BaseExtensions" = "disabled"
    "XDT_MicrosoftApplicationInsights_Mode"           = "recommended"
    "XDT_MicrosoftApplicationInsights_PreemptSdk"     = "disabled"
  }
  pension_data_app_settings = {
    "CommonServiceBusConfiguration__OutboundQueue"          = "pensions-retrieval-job"
    "PensionRetrievalServiceUrl"                            = "https://maps-api-management-${var.env}.azure-api.net/pension-retrieval-service/"
    "RetrievedPensionsServiceUrl"                           = "https://maps-api-management-${var.env}.azure-api.net/retrieved-pensions-record-service/"
    "tokenIntegrationServiceUrl"                            = "https://maps-api-management-${var.env}.azure-api.net/token-integration-service/"
    "MapsCdaServiceUrl"                                     = "https://maps-api-management-${var.env}.azure-api.net/maps-cda-service/"
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"                       = true
    "APPLICATIONINSIGHTS_CONNECTION_STRING"                 = "InstrumentationKey=${module.pension_data_service.instrumentation_key};IngestionEndpoint=https://uksouth-1.in.applicationinsights.azure.com/;LiveEndpoint=https://uksouth.livediagnostics.monitor.azure.com/;ApplicationId=${module.pension_data_service.app_insights_app_id}"
    "PeiRetrievalDuration"                                  = "60"
    "ViewDataRetrievalDuration"                             = "5"
    "CosmosBusinessConfiguration__DatabaseId"               = "mhpd-business-layer"
    "CosmosBusinessConfiguration__UserSessionDataContainer" = "mhpdUserSessionData"
  }
  token_integration_app_settings = {
    "APPINSIGHTS_INSTRUMENTATIONKEY"                    = module.token_integration_service.instrumentation_key
    "APPINSIGHTS_PROFILERFEATURE_VERSION"               = "1.0.0"
    "APPINSIGHTS_SNAPSHOTFEATURE_VERSION"               = "1.0.0"
    "APPLICATIONINSIGHTS_CONNECTION_STRING"             = "InstrumentationKey=${module.token_integration_service.instrumentation_key};IngestionEndpoint=https://uksouth-1.in.applicationinsights.azure.com/;LiveEndpoint=https://uksouth.livediagnostics.monitor.azure.com/;ApplicationId=${module.token_integration_service.app_insights_app_id}"
    "APPLICATIONINSIGHTS_ENABLESQLQUERYCOLLECTION"      = "disabled"
    "ApplicationInsightsAgent_EXTENSION_VERSION"        = "~2"
    "DISABLE_APPINSIGHTS_SDK"                           = "disabled"
    "DiagnosticServices_EXTENSION_VERSION"              = "~3"
    "IGNORE_APPINSIGHTS_SDK"                            = "disabled"
    "InstrumentationEngine_EXTENSION_VERSION"           = "disabled"
    "SnapshotDebugger_EXTENSION_VERSION"                = "disabled"
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"                   = "true"
    "XDT_MicrosoftApplicationInsights_BaseExtensions"   = "disabled"
    "XDT_MicrosoftApplicationInsights_Mode"             = "recommended"
    "XDT_MicrosoftApplicationInsights_PreemptSdk"       = "disabled"
    "CdaServiceUrl"                                     = "https://maps-api-management-${var.env}.azure-api.net/cda-integration-external/"
    "CosmosIntegrationConfiguration__DatabaseId"        = "mhpd-integration-layer"
    "CosmosIntegrationConfiguration__JwkCacheContainer" = "jwkUriEmulatorData"
  }
  cosmos_db_connection_string = {
    name  = "CosmosDBConnectionString"
    type  = "Custom"
    value = "AccountEndpoint=https://${var.product}-cosmos-${var.env}.documents.azure.com:443/;AccountKey=${data.azurerm_cosmosdb_account.this.primary_key}"
  }

  emulator_cosmos_db_connection_string = {
    name  = "CosmosDBConnectionString"
    type  = "Custom"
    value = "AccountEndpoint=https://${var.product}-cosmos-testharness.documents.azure.com:443/;AccountKey=${data.azurerm_cosmosdb_account.testharness.primary_key}"
  }
  service_bus_connection_string = {
    name  = "ServiceBusConnectionString"
    type  = "Custom"
    value = data.azurerm_servicebus_namespace.this.default_primary_connection_string
  }
}