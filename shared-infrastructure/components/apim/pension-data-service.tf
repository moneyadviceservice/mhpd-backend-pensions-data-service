resource "azurerm_api_management_api" "pension-data-service" {
  name                  = "pension-data-service"
  description           = "This API describes service that provides RQP tokens for clients interacting with CDA components."
  resource_group_name   = data.azurerm_api_management.this.resource_group_name
  api_management_name   = data.azurerm_api_management.this.name
  revision              = "1"
  display_name          = "pension-data-service"
  path                  = "pension-data-service"
  service_url           = "https://pension-data-service-${var.env}.azurewebsites.net"
  protocols             = ["https"]
  subscription_required = false
  subscription_key_parameter_names {
    header = "Ocp-Apim-Subscription-Key"
    query  = "subscription-key"
  }
  import {
    content_format = "openapi+json-link"
    content_value  = "https://raw.githubusercontent.com/moneyadviceservice/api-docs/refs/heads/add_api_docs/specs/pension-data.json"
  }
}

resource "azurerm_api_management_product_api" "pension-data-service" {
  api_name            = azurerm_api_management_api.pension-data-service.name
  product_id          = azurerm_api_management_product.mhpd.product_id
  api_management_name = data.azurerm_api_management.this.name
  resource_group_name = data.azurerm_api_management.this.resource_group_name

  depends_on = [azurerm_api_management_api.pension-data-service]
}

resource "azurerm_api_management_api_tag" "pension-data-service" {
  api_id = azurerm_api_management_api.pension-data-service.id
  name   = azurerm_api_management_tag.mhpd.name
}
