data "azurerm_resource_group" "mhpd" {
  name = "mhpd-${var.env}"
}

data "azurerm_cosmosdb_account" "this" {
  name                = "${var.product}-cosmos-${var.env}"
  resource_group_name = "${var.product}-${var.env}"
}

data "azurerm_servicebus_namespace" "this" {
  name                = "${var.product}-sbns-${var.env}"
  resource_group_name = "${var.product}-${var.env}"
}