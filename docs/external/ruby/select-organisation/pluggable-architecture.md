# Pluggable architecture

The library adopts a pluggable design, using lambda methods assigned to a configuration object to customise its components. For example:

```ruby
DfeSignIn::SelectOrganisation.configure do |config|
  config.middleware_factory = -> (config) {
    DfeSignIn::SelectOrganisation::StandardMiddleware.new(config)
  }
end
```

In this example, the `middleware_factory` lambda creates an instance of `StandardMiddleware`. Developers can replace this with a custom implementation that adheres to the required interface `DfeSignIn::SelectOrganisation::SelectOrganisationMiddleware`.

The library provides lambda functions for the following, each of which can receive a custom implementation. The example application uses `Standard*` implementations to demonstrate each of these.

```ruby
DfeSignIn::SelectOrganisation.configure do |config|
  config.middleware_factory = -> (config) {
    DfeSignIn::SelectOrganisation::StandardMiddleware.new(config)
  }
  config.flow_factory = -> (env) {
    DfeSignIn::SelectOrganisation::StandardUserFlow.new(env)
  }
  config.user_factory = -> (env) {
    DfeSignIn::SelectOrganisation::StandardUserProvider.new(env)
  }
  config.tracking_provider_factory = -> (env, session_provider) {
    DfeSignIn::SelectOrganisation::StandardTrackingProvider.new(session_provider)
  }
  config.events_provider_factory = -> (env, session_provider) {
    DfeSignIn::SelectOrganisation::StandardEvents.new(session_provider)
  }
  config.session_provider_factory = -> (env) {
    DfeSignIn::SelectOrganisation::StandardSessionProvider.new(env)
  }
end
```
