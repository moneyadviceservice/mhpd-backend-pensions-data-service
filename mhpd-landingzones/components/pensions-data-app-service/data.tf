data "azurerm_cosmosdb_account" "mhpd" {
  name                = "cosmos-${var.product}-${var.env}-uks"
  resource_group_name = "rg-${var.product}-${var.env}-uksouth"
}

data "azurerm_servicebus_namespace" "mhpd" {
  name                = "sbns-${var.product}-${var.env}-uks"
  resource_group_name = "rg-${var.product}-${var.env}-uksouth"
}
