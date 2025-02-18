data "azurerm_resource_group" "mhpd" {
  name = "${var.product}-${var.env}"
}

data "azurerm_client_config" "current" {}


data "azurerm_cosmosdb_account" "this" {
  name                = "${var.product}-cosmos-${var.env}"
  resource_group_name = "${var.product}-${var.env}"
}

data "azurerm_cosmosdb_account" "testharness" {
  name                = "${var.product}-cosmos-testharness"
  resource_group_name = "${var.product}-staging"
}

data "azurerm_servicebus_namespace" "this" {
  name                = "${var.product}-sbns-${var.env}"
  resource_group_name = "${var.product}-${var.env}"
}

data "azurerm_key_vault" "mhpd" {
  name                = "${var.product}-${var.env}"
  resource_group_name = "${var.product}-${var.env}"
}

data "azurerm_key_vault_secret" "pfx_password" {
  name         = "CryptopackSettings--PfxPassword"
  key_vault_id = data.azurerm_key_vault.mhpd.id
}

data "azurerm_key_vault_secret" "user_client_secret" {
  name         = "KeyVaultSettings--ClientSecret"
  key_vault_id = data.azurerm_key_vault.mhpd.id
}

data "azurerm_subnet" "mhpd-subnet" {
  for_each = var.env == "staging" || var.env == "prod" ? toset(["exists"]) : toset([])

  name                 = "${var.product}-apps"
  virtual_network_name = "maps-hub-vnet-${var.env}"
  resource_group_name  = "maps-hub-${var.env}"
}