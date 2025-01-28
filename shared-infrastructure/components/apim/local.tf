locals {
  pdp-backend-service       = "https://pdp-view-data-service-emulator-${var.env}.azurewebsites.net"
  pei-backend-service       = "https://pei-integration-service-${var.env}.azurewebsites.net"
  cda-backend-service       = "https://maps-api-management-${var.env}.azure-api.net/cda-emulator"
  view-data-backend-service = "@((string)context.Variables[\"backendUrl\"])"
  certificate-id            = "PdpMtls"
  backendUrl                = "@(context.Request.Headers.GetValueOrDefault(\"providerUrl\"))"

  api_management_logger_id = var.env == "prod" ? "/subscriptions/${data.azurerm_client_config.current.subscription_id}/resourceGroups/maps-apim-${var.env}/providers/Microsoft.ApiManagement/service/maps-api-management-${var.env}/loggers/apim-logger-prod" : "/subscriptions/${data.azurerm_client_config.current.subscription_id}/resourceGroups/maps-apim-${var.env}/providers/Microsoft.ApiManagement/service/maps-api-management-${var.env}/loggers/apim-logger-nonprod"
}