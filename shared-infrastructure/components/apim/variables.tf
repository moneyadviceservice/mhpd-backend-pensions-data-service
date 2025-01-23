variable "product" {
  default = "mhpd"
}

variable "env" {}

variable "subscriptions_limit" {
  default = 20
}

variable "sampling_percentage" {
  default = 5.0
}

variable "http_correlation_protocol" {
  default = "W3C"
}

variable "verbosity" {
  default = "verbose"
}