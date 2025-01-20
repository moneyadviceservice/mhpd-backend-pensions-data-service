resource "azurerm_api_management_api" "view-data-service-emulator" {
  name                  = "view-data-service-emulator"
  description           = "This API describes service that provides RQP tokens for clients interacting with CDA components."
  resource_group_name   = data.azurerm_api_management.this.resource_group_name
  api_management_name   = data.azurerm_api_management.this.name
  revision              = "1"
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
    content_value  = "https://raw.githubusercontent.com/moneyadviceservice/api-docs/refs/heads/add_api_docs/specs/view-data-emulator.json"
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
