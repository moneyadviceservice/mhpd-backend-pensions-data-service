resource "azurerm_api_management_api" "token-integration-service" {
  name                  = "token-integration-service"
  description           = "This API describes service that provides RQP tokens for clients interacting with CDA components."
  resource_group_name   = data.azurerm_api_management.this.resource_group_name
  api_management_name   = data.azurerm_api_management.this.name
  revision              = "2"
  display_name          = "token-integration-service"
  path                  = "token-integration-service"
  service_url           = "https://token-integration-service-${var.env}.azurewebsites.net"
  protocols             = ["https"]
  subscription_required = false
  subscription_key_parameter_names {
    header = "Ocp-Apim-Subscription-Key"
    query  = "subscription-key"
  }
  import {
    content_format = "openapi+json-link"
    content_value  = "https://raw.githubusercontent.com/moneyadviceservice/api-docs/refs/heads/add_api_docs/specs/token-integration.json"
  }
}

resource "azurerm_api_management_product_api" "token-integration-service" {
  api_name            = azurerm_api_management_api.token-integration-service.name
  product_id          = azurerm_api_management_product.mhpd.product_id
  api_management_name = data.azurerm_api_management.this.name
  resource_group_name = data.azurerm_api_management.this.resource_group_name

  depends_on = [azurerm_api_management_api.token-integration-service]
}

resource "azurerm_api_management_api_tag" "token-integration-service" {
  api_id = azurerm_api_management_api.token-integration-service.id
  name   = azurerm_api_management_tag.mhpd.name
}

resource "azurerm_api_management_api_diagnostic" "token-integration-service" {
  identifier                = "applicationinsights"
  resource_group_name       = data.azurerm_api_management.this.resource_group_name
  api_management_name       = data.azurerm_api_management.this.name
  api_name                  = azurerm_api_management_api.token-integration-service.name
  api_management_logger_id  = local.api_management_logger_id
  sampling_percentage       = var.sampling_percentage
  always_log_errors         = true
  log_client_ip             = true
  verbosity                 = var.verbosity
  http_correlation_protocol = var.http_correlation_protocol
}
