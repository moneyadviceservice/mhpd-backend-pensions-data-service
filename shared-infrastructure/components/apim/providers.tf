terraform {
  backend "azurerm" {}
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "4.4.0"
    }
  }
}

provider "azurerm" {
  skip_provider_registration = true
  #   resource_provider_registrations = none
  subscription_id = "3a9bae85-2f6e-47a1-a371-7ee3c84cf70b"
  features {
  }
}