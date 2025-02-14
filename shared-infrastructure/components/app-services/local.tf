locals {
  enable_vnet_integration = var.env == "staging" || var.env == "prod" ? true : false
  subnet_id               = var.env == "staging" || var.env == "prod" ? data.azurerm_subnet.mhpd-subnet.id : null
}