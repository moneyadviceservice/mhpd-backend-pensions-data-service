resource "azurerm_api_management_api" "cda-integration-external" {
  name                  = "cda-integration-external"
  description           = "This API describes service that provides RQP tokens for clients interacting with CDA components."
  resource_group_name   = data.azurerm_api_management.this.resource_group_name
  api_management_name   = data.azurerm_api_management.this.name
  revision              = "1"
  display_name          = "cda-integration-external"
  path                  = "cda-integration-external"
  service_url           = "https://cda-service-emulator-${var.env}.azurewebsites.net"
  protocols             = ["https"]
  subscription_required = false
  subscription_key_parameter_names {
    header = "Ocp-Apim-Subscription-Key"
    query  = "subscription-key"
  }
  import {
    content_format = "openapi+json-link"
    content_value  = "https://raw.githubusercontent.com/moneyadviceservice/api-docs/refs/heads/add_api_docs/specs/cda-integration-external.json"
  }
}

resource "azurerm_api_management_product_api" "cda-integration-external" {
  api_name            = azurerm_api_management_api.cda-integration-external.name
  product_id          = azurerm_api_management_product.mhpd.product_id
  api_management_name = data.azurerm_api_management.this.name
  resource_group_name = data.azurerm_api_management.this.resource_group_name

  depends_on = [azurerm_api_management_api.cda-integration-external]
}

resource "azurerm_api_management_api_tag" "cda-integration-external" {
  api_id = azurerm_api_management_api.cda-integration-external.id
  name   = azurerm_api_management_tag.pdp-ecosystem.name
}

resource "azurerm_api_management_api_policy" "cda-integration" {
  api_name            = azurerm_api_management_api.cda-integration-external.name
  api_management_name = data.azurerm_api_management.this.name
  resource_group_name = data.azurerm_api_management.this.resource_group_name

  xml_content = <<XML
<policies>
	<!-- Throttle, authorize, validate, cache, or transform the requests -->
	<inbound>
		<base />
		<set-backend-service base-url="${local.cda-backend-service}" />
		<authentication-certificate certificate-id="PdpMtls" />
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