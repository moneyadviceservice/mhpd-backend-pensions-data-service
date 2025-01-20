resource "azurerm_api_management_api" "cda-redirect-details" {
  name                  = "cda-redirect-details"
  description           = "Wrapper of the cda emulator services enforcing mTLS"
  resource_group_name   = data.azurerm_api_management.this.resource_group_name
  api_management_name   = data.azurerm_api_management.this.name
  revision              = "1"
  display_name          = "cda-redirect-details"
  path                  = "cda-redirect-details"
  service_url           = "https://cda-service-emulator-${var.env}.azurewebsites.net"
  protocols             = ["https"]
  subscription_required = false
  subscription_key_parameter_names {
    header = "Ocp-Apim-Subscription-Key"
    query  = "subscription-key"
  }
  import {
    content_format = "openapi-link"
    content_value  = "https://raw.githubusercontent.com/moneyadviceservice/api-docs/refs/heads/add_api_docs/specs/cda-redirect-details.json"
  }
}

resource "azurerm_api_management_product_api" "cda-redirect-details" {
  api_name            = azurerm_api_management_api.cda-redirect-details.name
  product_id          = azurerm_api_management_product.mhpd.product_id
  api_management_name = data.azurerm_api_management.this.name
  resource_group_name = data.azurerm_api_management.this.resource_group_name

  depends_on = [azurerm_api_management_api.cda-redirect-details]
}

resource "azurerm_api_management_api_tag" "cda-redirect-details" {
  api_id = azurerm_api_management_api.cda-redirect-details.id
  name   = azurerm_api_management_tag.mhpd.name
}