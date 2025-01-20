resource "azurerm_api_management_tag" "test-harness" {
  api_management_id = data.azurerm_api_management.this.id
  name              = "test-harness"
}

resource "azurerm_api_management_tag" "pdp-ecosystem" {
  api_management_id = data.azurerm_api_management.this.id
  name              = "pdp-ecosystem"
}

resource "azurerm_api_management_tag" "mhpd" {
  api_management_id = data.azurerm_api_management.this.id
  name              = var.product
}

resource "azurerm_api_management_tag" "token-integration" {
  api_management_id = data.azurerm_api_management.this.id
  name              = "token-integration"
}

resource "azurerm_api_management_tag" "retrieved-pensions" {
  api_management_id = data.azurerm_api_management.this.id
  name              = "retrieved-pensions"
}

resource "azurerm_api_management_tag" "pensions-retrieval" {
  api_management_id = data.azurerm_api_management.this.id
  name              = "pensions-retrieval"
}

resource "azurerm_api_management_tag" "pei-integration" {
  api_management_id = data.azurerm_api_management.this.id
  name              = "pei-integration"
}

resource "azurerm_api_management_tag" "maps-cda" {
  api_management_id = data.azurerm_api_management.this.id
  name              = "maps-cda"
}

resource "azurerm_api_management_tag" "cda-emulator" {
  api_management_id = data.azurerm_api_management.this.id
  name              = "cda-emulator"
}
