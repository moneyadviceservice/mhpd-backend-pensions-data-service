data "azurerm_client_config" "current" {}

data "azurerm_resource_group" "this" {
  name = "${var.product}-${var.env}"
}

data "azurerm_key_vault" "mhpd" {
  name                = "${var.product}-${var.env}"
  resource_group_name = "${var.product}-${var.env}"
}

data "azurerm_key_vault_secret" "maps_cda_service_private_key" {
  name         = "maps-cda-service-private-key"
  key_vault_id = data.azurerm_key_vault.mhpd.id
}

data "azurerm_key_vault_secret" "maps_cda_service_expiry" {
  name         = "maps-cda-service-expiry"
  key_vault_id = data.azurerm_key_vault.mhpd.id
}

data "azurerm_key_vault_secret" "maps_cda_service_audience" {
  name         = "maps-cda-service-audience"
  key_vault_id = data.azurerm_key_vault.mhpd.id
}

data "azurerm_key_vault_secret" "maps_cda_service_kid" {
  name         = "maps-cda-service-kid"
  key_vault_id = data.azurerm_key_vault.mhpd.id
}

data "azurerm_key_vault_secret" "cda_service_emulator_private_key" {
  name         = "cda-service-emulator-private-key"
  key_vault_id = data.azurerm_key_vault.mhpd.id
}

data "azurerm_key_vault_secret" "jwt_settings_kid" {
  name         = "jwt-settings-kid"
  key_vault_id = data.azurerm_key_vault.mhpd.id
}

data "azurerm_key_vault_secret" "jwt_settings_subject" {
  name         = "jwt-settings-subject"
  key_vault_id = data.azurerm_key_vault.mhpd.id
}

data "azurerm_servicebus_namespace" "this" {
  name                = "${var.product}-sbns-${var.env}"
  resource_group_name = "${var.product}-${var.env}"
}

data "azurerm_cosmosdb_account" "this" {
  name                = "${var.product}-cosmos-${var.env}"
  resource_group_name = "${var.product}-${var.env}"
}

data "azurerm_subnet" "mhpd-subnet" {
  for_each = var.env == "staging" || var.env == "prod" ? toset(["exists"]) : toset([])

  name                 = "${var.product}-apps"
  virtual_network_name = "maps-hub-vnet-${var.env}"
  resource_group_name  = "maps-hub-${var.env}"
}