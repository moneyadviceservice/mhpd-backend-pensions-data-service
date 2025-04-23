locals {
  enable_vnet_integration = var.env == "staging" || var.env == "prod" ? true : false
  subnet_id               = length(data.azurerm_subnet.mhpd-subnet) > 0 ? data.azurerm_subnet.mhpd-subnet["exists"].id : null
  # create_service_plan     = var.env == "staging" || var.env == "prod" ? true : false
}