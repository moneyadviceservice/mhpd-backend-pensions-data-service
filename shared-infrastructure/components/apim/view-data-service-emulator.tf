resource "azurerm_api_management_api" "view-data-service-emulator" {
  name                  = "view-data-service-emulator"
  description           = "This API describes service that provides RQP tokens for clients interacting with CDA components."
  resource_group_name   = data.azurerm_api_management.this.resource_group_name
  api_management_name   = data.azurerm_api_management.this.name
  revision              = "5"
  display_name          = "view-data-service-emulator"
  path                  = "view-data-service-emulator"
  service_url           = "https://pdp-view-data-service-emulator-${var.env}.azurewebsites.net"
  protocols             = ["https"]
  subscription_required = false
  subscription_key_parameter_names {
    header = "Ocp-Apim-Subscription-Key"
    query  = "subscription-key"
  }
  import {
    content_format = "openapi+json-link"
    content_value  = "https://raw.githubusercontent.com/moneyadviceservice/api-docs/refs/heads/main/specs/view-data-emulator.json"
  }
}

resource "azurerm_api_management_product_api" "view-data-service-emulator" {
  api_name            = azurerm_api_management_api.view-data-service-emulator.name
  product_id          = azurerm_api_management_product.mhpd.product_id
  api_management_name = data.azurerm_api_management.this.name
  resource_group_name = data.azurerm_api_management.this.resource_group_name

  depends_on = [azurerm_api_management_api.view-data-service-emulator]
}

resource "azurerm_api_management_api_tag" "view-data-service-emulator-test-harness" {
  api_id = azurerm_api_management_api.view-data-service-emulator.id
  name   = azurerm_api_management_tag.test-harness.name
}

resource "azurerm_api_management_api_diagnostic" "view-data-service-emulator" {
  identifier                = "applicationinsights"
  resource_group_name       = data.azurerm_api_management.this.resource_group_name
  api_management_name       = data.azurerm_api_management.this.name
  api_name                  = azurerm_api_management_api.view-data-service-emulator.name
  api_management_logger_id  = local.api_management_logger_id
  sampling_percentage       = var.sampling_percentage
  always_log_errors         = true
  log_client_ip             = true
  verbosity                 = var.verbosity
  http_correlation_protocol = var.http_correlation_protocol
}


resource "azurerm_api_management_api_policy" "view-data-service-emulator" {
  api_name            = azurerm_api_management_api.view-data-service-emulator.name
  api_management_name = data.azurerm_api_management.this.name
  resource_group_name = data.azurerm_api_management.this.resource_group_name

  xml_content = <<XML
<policies>
    <!-- Throttle, authorize, validate, cache, or transform the requests -->
    <inbound>
        <base />
        <choose>
            <when condition="@(context.Request.Certificate == null)">
                <return-response>
                    <set-status code="403" reason="Forbidden" />
                    <set-body>@{
                        return "Access denied: Client certificate is missing.";
                    }</set-body>
                </return-response>
            </when>
            <when condition="@(!context.Request.Certificate.Subject.Contains("CN=casstubclient"))">
                <return-response>
                    <set-status code="403" reason="Forbidden" />
                    <set-body>@{
                        return "Access denied: Client certificate is invalid.";
                    }</set-body>
                </return-response>
            </when>
        </choose>
    </inbound>
    <!-- Control if and how the requests are forwarded to services  -->
    <backend>
        <base />
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
