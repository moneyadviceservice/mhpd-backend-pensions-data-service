resource "azurerm_api_management_api" "cda-pei-retrieval" {
  name                  = "cda-pei-retrieval"
  description           = "Wrapper of the cda emulator services enforcing mTLS"
  resource_group_name   = data.azurerm_api_management.this.resource_group_name
  api_management_name   = data.azurerm_api_management.this.name
  revision              = "1"
  display_name          = "cda-pei-retrieval"
  path                  = "cda-emulator"
  service_url           = "https://cda-service-emulator-${var.env}.azurewebsites.net"
  protocols             = ["https"]
  subscription_required = false
  subscription_key_parameter_names {
    header = "Ocp-Apim-Subscription-Key"
    query  = "subscription-key"
  }
  import {
    content_format = "openapi-link"
    content_value  = "https://raw.githubusercontent.com/moneyadviceservice/api-docs/refs/heads/add_api_docs/specs/cda-pei-retrieval-details.json"
  }
}

resource "azurerm_api_management_product_api" "cda-pei-retrieval" {
  api_name            = azurerm_api_management_api.cda-pei-retrieval.name
  product_id          = azurerm_api_management_product.mhpd.product_id
  api_management_name = data.azurerm_api_management.this.name
  resource_group_name = data.azurerm_api_management.this.resource_group_name

  depends_on = [azurerm_api_management_api.cda-pei-retrieval]
}

resource "azurerm_api_management_api_tag" "cda-pei-retrieval" {
  api_id = azurerm_api_management_api.cda-pei-retrieval.id
  name   = azurerm_api_management_tag.mhpd.name
}