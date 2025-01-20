resource "azurerm_api_management_api" "maps-cda-service" {
  name                  = "maps-cda-service"
  description           = "This API describes service that provides RQP tokens for clients interacting with CDA components."
  resource_group_name   = data.azurerm_api_management.this.resource_group_name
  api_management_name   = data.azurerm_api_management.this.name
  revision              = "1"
  display_name          = "maps-cda-service"
  path                  = "maps-cda-service"
  service_url           = "https://cda-service-${var.env}.azurewebsites.net"
  protocols             = ["https"]
  subscription_required = false
  subscription_key_parameter_names {
    header = "Ocp-Apim-Subscription-Key"
    query  = "subscription-key"
  }
  import {
    content_format = "openapi+json-link"
    content_value  = "https://raw.githubusercontent.com/moneyadviceservice/api-docs/refs/heads/add_api_docs/specs/maps-cda.json"
  }
}

resource "azurerm_api_management_product_api" "maps-cda-service" {
  api_name            = azurerm_api_management_api.maps-cda-service.name
  product_id          = azurerm_api_management_product.mhpd.product_id
  api_management_name = data.azurerm_api_management.this.name
  resource_group_name = data.azurerm_api_management.this.resource_group_name

  depends_on = [azurerm_api_management_api.maps-cda-service]
}

resource "azurerm_api_management_api_tag" "maps-cda-service" {
  api_id = azurerm_api_management_api.maps-cda-service.id
  name   = azurerm_api_management_tag.mhpd.name
}

# resource "azurerm_api_management_api_policy" "forward-request-cda-service" {
#   api_name            = azurerm_api_management_api.maps-cda-service.name
#   api_management_name = data.azurerm_api_management.this.name
#   resource_group_name = data.azurerm_api_management.this.resource_group_name

#   xml_content = <<XML
# <policies>
#     <!-- Throttle, authorize, validate, cache, or transform the requests -->
#     <inbound>
#         <base />
#     </inbound>
#     <!-- Control if and how the requests are forwarded to services  -->
#     <backend>
#         <forward-request />
#     </backend>
#     <!-- Customize the responses -->
#     <outbound>
#         <base />
#     </outbound>
#     <!-- Handle exceptions and customize error responses  -->
#     <on-error>
#         <base />
#     </on-error>
# </policies>
# XML
# }