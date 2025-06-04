# What is API client?

`DfeSignIn::ApiClient` is a Ruby library designed to simplify interaction with the DfE public API. It provides a wrapped HTTP client `DfeSignIn::ApiClient::HttpClient`, authentication management, and endpoint helpers.

> **Note:** To use the DfE Sign-in public API and its endpoints you must provide an Authorization header with a Bearer token formatted as a JSON Web Token (JWT). The JWT must be signed using a secret known by both DfE Sign-in and the service, otherwise requests will fail with a status code of 403.

## Overview

The `DfeSignIn::ApiClient` library offers the following key features:

- `DfeSignIn::ApiClient::HttpClient` Wrapped HTTP client that automatically includes a JSON Web Token (JWT) in the Authorization header of HTTP requests.

- `DfeSignIn::ApiClient::BearerTokenManager` Handles JWT generation and renewal.

- `DfeSignIn::ApiClient::EndpointBase` All endpoints inherit EndpointBase which provides access to `DfeSignIn::ApiClient::HttpClient` for making requests to the DfE public API.

- `DfeSignIn::ApiClient::OrganisationEndpoint` Defines endpoints relating to Organisation specific operations.

## Example

### Configuration

```ruby
  DfeSignIn::ApiClient.configure do |config|
    config.service_url = "https://dsi-service-url.com"
    config.jwt_iss = "example-service-iss"
    config.jwt_aud = "signin.education.gov.uk"
    config.jwt_secret = "jwt_secret"
    config.jwt_exp_timeframe = 3600
  end
```

### Usage

Return the details of an Organisation.

```ruby
  DfeSignIn::ApiClient.organisation.get_details({
    user_id: "",
    organisation_id: "",
    ...
  })
```
