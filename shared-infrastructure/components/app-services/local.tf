locals {
  enable_vnet_integration    = var.env == "staging" || var.env == "prod" ? true : false
  subnet_id                  = length(data.azurerm_subnet.mhpd-subnet) > 0 ? data.azurerm_subnet.mhpd-subnet["exists"].id : null
  sku_name                   = var.env == "staging" || var.env == "prod" ? "P1v3" : "B3"
  cda_asp_name               = var.env == "staging" || var.env == "prod" ? "cda-service-asp-${var.env}" : "${var.product}-asp-${var.env}"
  pei_asp_name               = var.env == "staging" || var.env == "prod" ? "pei-integration-asp-${var.env}" : "${var.product}-asp-${var.env}"
  pension_data_asp_name      = var.env == "staging" || var.env == "prod" ? "Pension-data-asp-${var.env}" : "${var.product}-asp-${var.env}"
  token_integration_asp_name = var.env == "staging" || var.env == "prod" ? "token-integration-asp-${var.env}" : "${var.product}-asp-${var.env}"
  create_service_plan        = var.env == "staging" || var.env == "prod" ? true : false
}