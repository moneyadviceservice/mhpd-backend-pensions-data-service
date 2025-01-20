data "azurerm_client_config" "current" {}

data "azurerm_api_management" "this" {
  name                = "maps-api-management-${var.env}"
  resource_group_name = "maps-apim-${var.env}"
}

data "azurerm_key_vault" "mhpd" {
  name                = "${var.product}-${var.env}"
  resource_group_name = "${var.product}-${var.env}"
}

data "azurerm_key_vault_certificate" "pdp-mtls" {
  name         = "PdpMtls"
  key_vault_id = data.azurerm_key_vault.mhpd.id
}
