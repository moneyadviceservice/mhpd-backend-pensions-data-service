data "azurerm_client_config" "current" {}

data "azurerm_resource_group" "this" {
  name = "${var.product}-${var.env}"
}

data "azurerm_key_vault" "mhpd" {
  name                = "${var.product}-${var.env}"
  resource_group_name = "${var.product}-${var.env}"
}


data "azurerm_key_vault_secret" "cda_service_private_key" {
  name         = "CDA-service-private-key"
  key_vault_id = data.azurerm_key_vault.mhpd.id
}

data "azurerm_key_vault_secret" "CDA_service_emulator_private_key" {
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

data "azurerm_servicebus_namespace" "pension_data" {
  name                = "${var.product}-sbns-${var.env}"
  resource_group_name = "${var.product}-${var.env}"
}

data "azurerm_cosmosdb_account" "this" {
  name                = "${var.product}-cosmos-${var.env}"
  resource_group_name = "${var.product}-${var.env}"
}
