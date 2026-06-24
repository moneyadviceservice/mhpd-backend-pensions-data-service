resource "azurerm_service_plan" "this" {
  name                   = local.service_plan_name
  resource_group_name    = local.resource_group_name
  location               = var.location
  os_type                = "Linux"
  sku_name               = local.sku_name
  zone_balancing_enabled = local.zone_redundant
  worker_count           = local.zone_redundant ? 3 : null

  lifecycle {
    ignore_changes = [tags]
  }
}

resource "azurerm_linux_web_app" "this" {
  name                          = local.app_name
  resource_group_name           = local.resource_group_name
  location                      = azurerm_service_plan.this.location
  service_plan_id               = azurerm_service_plan.this.id
  app_settings                  = local.pension_data_app_settings
  client_affinity_enabled       = true
  https_only                    = true
  public_network_access_enabled = true
  virtual_network_subnet_id     = local.enable_vnet_integration ? local.apps_subnet_id : null
  tags                          = {}

  connection_string {
    name  = local.cosmos_db_connection_string.name
    type  = local.cosmos_db_connection_string.type
    value = local.cosmos_db_connection_string.value
  }

  connection_string {
    name  = local.service_bus_connection_string.name
    type  = local.service_bus_connection_string.type
    value = local.service_bus_connection_string.value
  }

  identity {
    type = "SystemAssigned"
  }

  site_config {
    ip_restriction_default_action = local.ip_restriction_default_action
    ftps_state                    = var.ftps_state
    app_command_line              = "dotnet PensionsDataService.dll"
    vnet_route_all_enabled        = local.enable_vnet_integration ? true : null

    application_stack {
      dotnet_version = "10.0"
    }

    dynamic "ip_restriction" {
      for_each = local.ip_restrictions
      content {
        name                      = ip_restriction.value.name
        priority                  = ip_restriction.value.priority
        action                    = ip_restriction.value.action
        virtual_network_subnet_id = ip_restriction.value.virtual_network_subnet_id
        headers                   = ip_restriction.value.headers
        ip_address                = ip_restriction.value.ip_address
      }
    }
  }

  lifecycle {
    ignore_changes = [tags]
  }
}

resource "azurerm_monitor_autoscale_setting" "this" {
  name                = azurerm_service_plan.this.name
  resource_group_name = local.resource_group_name
  location            = var.location
  target_resource_id  = azurerm_service_plan.this.id

  profile {
    name = "Scale out condition"
    capacity {
      default = 2
      minimum = 2
      maximum = 30
    }

    rule {
      metric_trigger {
        metric_name        = "CpuPercentage"
        metric_resource_id = azurerm_service_plan.this.id
        time_grain         = "PT1M"
        statistic          = "Average"
        time_window        = "PT5M"
        time_aggregation   = "Average"
        operator           = "GreaterThan"
        threshold          = 60
      }
      scale_action {
        direction = "Increase"
        type      = "ChangeCount"
        value     = "2"
        cooldown  = "PT5M"
      }
    }

    rule {
      metric_trigger {
        metric_name        = "CpuPercentage"
        metric_resource_id = azurerm_service_plan.this.id
        time_grain         = "PT1M"
        statistic          = "Average"
        time_window        = "PT5M"
        time_aggregation   = "Average"
        operator           = "LessThan"
        threshold          = 40
      }
      scale_action {
        direction = "Decrease"
        type      = "ChangeCount"
        value     = "2"
        cooldown  = "PT2M"
      }
    }
  }

  lifecycle {
    ignore_changes = [tags]
  }
}

resource "azurerm_linux_web_app_slot" "staging" {
  count          = var.env == "prod" ? 1 : 0
  name           = "staging"
  app_service_id = azurerm_linux_web_app.this.id
  app_settings   = local.pension_data_app_settings

  client_affinity_enabled       = true
  https_only                    = true
  public_network_access_enabled = true
  virtual_network_subnet_id     = local.apps_subnet_id

  connection_string {
    name  = local.cosmos_db_connection_string.name
    type  = local.cosmos_db_connection_string.type
    value = local.cosmos_db_connection_string.value
  }

  connection_string {
    name  = local.service_bus_connection_string.name
    type  = local.service_bus_connection_string.type
    value = local.service_bus_connection_string.value
  }

  identity {
    type = "SystemAssigned"
  }

  site_config {
    ip_restriction_default_action = "Deny"
    ftps_state                    = var.ftps_state
    app_command_line              = "dotnet PensionsDataService.dll"
    vnet_route_all_enabled        = true

    application_stack {
      dotnet_version = "10.0"
    }

    dynamic "ip_restriction" {
      for_each = local.ip_restrictions
      content {
        name                      = ip_restriction.value.name
        priority                  = ip_restriction.value.priority
        action                    = ip_restriction.value.action
        virtual_network_subnet_id = ip_restriction.value.virtual_network_subnet_id
        headers                   = ip_restriction.value.headers
        ip_address                = ip_restriction.value.ip_address
      }
    }
  }

  lifecycle {
    ignore_changes = [tags]
  }
}

resource "azurerm_application_insights" "this" {
  name                = local.app_name
  location            = var.location
  resource_group_name = local.resource_group_name

  application_type                      = "web"
  daily_data_cap_in_gb                  = 50
  sampling_percentage                   = 100
  workspace_id                          = local.logs_workspace_id
  retention_in_days                     = 90
  daily_data_cap_notifications_disabled = true

  lifecycle {
    ignore_changes = [tags]
  }
}
