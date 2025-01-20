resource "azurerm_api_management_api" "jwk" {
  name                  = "jwk"
  description           = "This API describes service that provides RQP tokens for clients interacting with CDA components."
  resource_group_name   = data.azurerm_api_management.this.resource_group_name
  api_management_name   = data.azurerm_api_management.this.name
  revision              = "1"
  display_name          = "jwk"
  path                  = "jwk"
  service_url           = "https://cda-service-emulator-${var.env}.azurewebsites.net"
  protocols             = ["https"]
  subscription_required = false
  subscription_key_parameter_names {
    header = "Ocp-Apim-Subscription-Key"
    query  = "subscription-key"
  }
  import {
    content_format = "openapi-link"
    content_value  = "https://raw.githubusercontent.com/moneyadviceservice/api-docs/refs/heads/add_api_docs/specs/jwk.json"
  }
}

resource "azurerm_api_management_product_api" "jwk" {
  api_name            = azurerm_api_management_api.jwk.name
  product_id          = azurerm_api_management_product.mhpd.product_id
  api_management_name = data.azurerm_api_management.this.name
  resource_group_name = data.azurerm_api_management.this.resource_group_name

  depends_on = [azurerm_api_management_api.jwk]
}

resource "azurerm_api_management_api_tag" "jwk-emulator" {
  api_id = azurerm_api_management_api.jwk.id
  name   = azurerm_api_management_tag.cda-emulator.name
}

resource "azurerm_api_management_api_tag" "jwk" {
  api_id = azurerm_api_management_api.jwk.id
  name   = azurerm_api_management_tag.pdp-ecosystem.name
}