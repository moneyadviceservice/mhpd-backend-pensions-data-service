locals {
  pdp-backend-service = "https://pdp-view-data-service-emulator-${var.env}.azurewebsites.net"
  pei-backend-service = "https://pei-integration-service-${var.env}.azurewebsites.net"
  cda-backend-service = "https://cda-service-emulator-${var.env}.azurewebsites.net"
}