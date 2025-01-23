resource "azurerm_api_management_api" "retrieved-pensions-record-service" {
  name                  = "retrieved-pensions-record-service"
  description           = "This service allows a client to retrieve retrieved pension records related to a user session"
  resource_group_name   = data.azurerm_api_management.this.resource_group_name
  api_management_name   = data.azurerm_api_management.this.name
  revision              = "1"
  display_name          = "retrieved-pensions-record-service"
  path                  = "retrieved-pensions-record-service"
  service_url           = "https://func-pension-request-${var.env}.azurewebsites.net"
  protocols             = ["https"]
  subscription_required = false
  subscription_key_parameter_names {
    header = "Ocp-Apim-Subscription-Key"
    query  = "subscription-key"
  }
  import {
    content_format = "openapi+json-link"
    content_value  = "https://raw.githubusercontent.com/moneyadviceservice/api-docs/refs/heads/add_api_docs/specs/retrieved-pensions-record.json"
  }
}

resource "azurerm_api_management_product_api" "retrieved-pensions-record-service" {
  api_name            = azurerm_api_management_api.retrieved-pensions-record-service.name
  product_id          = azurerm_api_management_product.mhpd.product_id
  api_management_name = data.azurerm_api_management.this.name
  resource_group_name = data.azurerm_api_management.this.resource_group_name
  depends_on          = [azurerm_api_management_api.retrieved-pensions-record-service]
}

resource "azurerm_api_management_api_tag" "retrieved-pensions-record-service" {
  api_id = azurerm_api_management_api.retrieved-pensions-record-service.id
  name   = azurerm_api_management_tag.mhpd.name
}

resource "azurerm_api_management_api_policy" "forward-request-retrieved-pension" {
  api_name            = azurerm_api_management_api.retrieved-pensions-record-service.name
  api_management_name = data.azurerm_api_management.this.name
  resource_group_name = data.azurerm_api_management.this.resource_group_name

  xml_content = <<XML
<policies>
    <!-- Throttle, authorize, validate, cache, or transform the requests -->
    <inbound>
        <base />
    </inbound>
    <!-- Control if and how the requests are forwarded to services  -->
    <backend>
        <forward-request />
    </backend>
    <!-- Customize the responses -->
    <outbound>
        <base />
    </outbound>
    <!-- Handle exceptions and customize error responses  -->
    <on-error>
        <base />
    </on-error>
</policies>
XML
}

resource "azurerm_api_management_api_diagnostic" "retrieved-pensions-record-service" {
  identifier                = "applicationinsights"
  resource_group_name       = data.azurerm_api_management.this.resource_group_name
  api_management_name       = data.azurerm_api_management.this.name
  api_name                  = azurerm_api_management_api.retrieved-pensions-record-service.name
  api_management_logger_id  = local.api_management_logger_id
  sampling_percentage       = var.sampling_percentage
  always_log_errors         = true
  log_client_ip             = true
  verbosity                 = var.verbosity
  http_correlation_protocol = var.http_correlation_protocol
}
