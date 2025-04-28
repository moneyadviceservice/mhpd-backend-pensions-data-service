resource "azurerm_api_management_api" "cda-emulator" {
  name                  = "cda-emulator"
  description           = "Wrapper of the CDA emulator services enforcing mTLS"
  resource_group_name   = data.azurerm_api_management.this.resource_group_name
  api_management_name   = data.azurerm_api_management.this.name
  revision              = "2"
  display_name          = "cda-emulator"
  path                  = "cda-emulator"
  service_url           = "https://cda-service-emulator-${var.env}.azurewebsites.net"
  protocols             = ["https"]
  subscription_required = false
  subscription_key_parameter_names {
    header = "Ocp-Apim-Subscription-Key"
    query  = "subscription-key"
  }
  import {
    content_format = "openapi+json-link"
    content_value  = "https://raw.githubusercontent.com/moneyadviceservice/api-docs/refs/heads/main/specs/cda-emulator.json"
  }
}

resource "azurerm_api_management_product_api" "cda-emulator" {
  api_name            = azurerm_api_management_api.cda-emulator.name
  product_id          = azurerm_api_management_product.mhpd.product_id
  api_management_name = data.azurerm_api_management.this.name
  resource_group_name = data.azurerm_api_management.this.resource_group_name

  depends_on = [azurerm_api_management_api.cda-emulator]
}

resource "azurerm_api_management_api_tag" "cda-emulator-test-harness" {
  api_id = azurerm_api_management_api.cda-emulator.id
  name   = azurerm_api_management_tag.test-harness.name
}

resource "azurerm_api_management_api_diagnostic" "cda-emulator" {
  identifier                = "applicationinsights"
  resource_group_name       = data.azurerm_api_management.this.resource_group_name
  api_management_name       = data.azurerm_api_management.this.name
  api_name                  = azurerm_api_management_api.cda-emulator.name
  api_management_logger_id  = local.api_management_logger_id
  sampling_percentage       = var.sampling_percentage
  always_log_errors         = true
  log_client_ip             = true
  verbosity                 = var.verbosity
  http_correlation_protocol = var.http_correlation_protocol
}

resource "azurerm_api_management_api_policy" "cda-emulator" {
  api_name            = azurerm_api_management_api.cda-emulator.name
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
            <!--otherwise>
                <validate-client-certificate validate-revocation="true" validate-trust="true" validate-not-before="true" validate-not-after="true" ignore-error="false">
                    <identities>
                        <identity issuer-certificate-id="${local.certificate-id}" />
                    </identities>
                </validate-client-certificate>
            </otherwise-->
        </choose>
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
