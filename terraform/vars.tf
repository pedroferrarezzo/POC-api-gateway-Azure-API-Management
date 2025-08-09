variable "appname" {
  default = "simpleapiauth"
}

variable "location" {
  default = "East US"
}

variable "docker_image" {
  default = "ferrarezzodev/apim-poc:latest"
}

variable "docker_registry_url" {
  default = "https://index.docker.io"
}

variable "subscription_id" {
}

variable "jwt_secret" {
}