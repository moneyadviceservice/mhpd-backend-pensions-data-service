resource "azurerm_api_management_api" "cda-emulator" {
  name                  = "cda-emulator"
  description           = "Wrapper of the CDA emulator services enforcing mTLS"
  resource_group_name   = data.azurerm_api_management.this.resource_group_name
  api_management_name   = data.azurerm_api_management.this.name
  revision              = "1"
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
    content_value  = "https://raw.githubusercontent.com/moneyadviceservice/api-docs/refs/heads/add_api_docs/specs/cda-emulator.json"
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
