resource "azurerm_api_management_api" "peis" {
  name                  = "peis"
  description           = "This API describes service that provides RQP tokens for clients interacting with CDA components."
  resource_group_name   = data.azurerm_api_management.this.resource_group_name
  api_management_name   = data.azurerm_api_management.this.name
  revision              = "1"
  display_name          = "peis"
  path                  = "peis"
  service_url           = "https://cda-service-emulator-${var.env}.azurewebsites.net"
  protocols             = ["https"]
  subscription_required = false
  subscription_key_parameter_names {
    header = "Ocp-Apim-Subscription-Key"
    query  = "subscription-key"
  }
  import {
    content_format = "openapi-link"
    content_value  = "https://raw.githubusercontent.com/moneyadviceservice/api-docs/refs/heads/add_api_docs/specs/peis.json"
  }
}

resource "azurerm_api_management_product_api" "peis" {
  api_name            = azurerm_api_management_api.peis.name
  product_id          = azurerm_api_management_product.mhpd.product_id
  api_management_name = data.azurerm_api_management.this.name
  resource_group_name = data.azurerm_api_management.this.resource_group_name

  depends_on = [azurerm_api_management_api.peis]
}

resource "azurerm_api_management_api_tag" "peis-mhpd" {
  api_id = azurerm_api_management_api.peis.id
  name   = azurerm_api_management_tag.mhpd.name
}

resource "azurerm_api_management_api_tag" "peis-emulator" {
  api_id = azurerm_api_management_api.peis.id
  name   = azurerm_api_management_tag.cda-emulator.name
}

resource "azurerm_api_management_api_tag" "peis-pdp-ecosystem" {
  api_id = azurerm_api_management_api.peis.id
  name   = azurerm_api_management_tag.pdp-ecosystem.name
}