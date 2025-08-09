resource "azurerm_resource_group" "rg" {
  name     = "${var.appname}-rg"
  location = var.location
}

resource "azurerm_service_plan" "asp" {
  name                = "${var.appname}-asp"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  os_type             = "Linux"
  sku_name            = "B1" # Plano básico barato para dev
}

resource "azurerm_linux_web_app" "app" {
  name                = "${var.appname}-app"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  service_plan_id     = azurerm_service_plan.asp.id

  site_config {
    application_stack {
      docker_image_name     = var.simpleapiauth_docker_image_name
      docker_registry_url   = var.docker_registry_url
    }
  }

    app_settings = {
    "Jwt__Key" = var.jwt_secret
  }

  https_only = false # apenas dev
}