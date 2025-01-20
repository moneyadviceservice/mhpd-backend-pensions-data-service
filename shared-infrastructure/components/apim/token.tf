resource "azurerm_api_management_api" "token" {
  name                  = "token"
  description           = "This API describes service that provides RQP tokens for clients interacting with CDA components."
  resource_group_name   = data.azurerm_api_management.this.resource_group_name
  api_management_name   = data.azurerm_api_management.this.name
  revision              = "1"
  display_name          = "token"
  path                  = "token"
  service_url           = "https://cda-service-emulator-${var.env}.azurewebsites.net"
  protocols             = ["https"]
  subscription_required = false
  subscription_key_parameter_names {
    header = "Ocp-Apim-Subscription-Key"
    query  = "subscription-key"
  }
  import {
    content_format = "openapi-link"
    content_value  = "https://raw.githubusercontent.com/moneyadviceservice/api-docs/refs/heads/add_api_docs/specs/token.json"
  }
}

resource "azurerm_api_management_product_api" "token" {
  api_name            = azurerm_api_management_api.token.name
  product_id          = azurerm_api_management_product.mhpd.product_id
  api_management_name = data.azurerm_api_management.this.name
  resource_group_name = data.azurerm_api_management.this.resource_group_name

  depends_on = [azurerm_api_management_api.token]
}

resource "azurerm_api_management_api_tag" "token-emulator" {
  api_id = azurerm_api_management_api.token.id
  name   = azurerm_api_management_tag.cda-emulator.name
}

resource "azurerm_api_management_api_tag" "token" {
  api_id = azurerm_api_management_api.token.id
  name   = azurerm_api_management_tag.pdp-ecosystem.name
}