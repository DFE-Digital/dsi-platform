# Introduction

The `DfeSignIn::SelectOrganisation::Middleware` library is a Rack middleware designed to manage the "selection organisation" flow. It intercepts specific URL paths to perform two primary functions:

1. Initiating a session with the DfE Public API to enable users to select an organisation.
2. Processing the response once the user has made their selection.

This library provides a flexible, pluggable interface that allows developers to customise its behavior to meet specific service requirements. It includes example implementations that demonstrate how to achieve these tasks.

> **Note:** As with any code examples, you should always follow your established guidelines for software development, along with system and data security.

### Pluggable Architecture

The library adopts a pluggable design, using lambda methods assigned to a configuration object to customise its components. For example:

```ruby
  DfeSignIn::SelectOrganisation.configure do |config|
    config.middleware_factory = -> (config) {
      DfeSignIn::SelectOrganisation::StandardMiddleware.new(config)
    }
  end
```

In this example, the `middleware_factory` lambda creates an instance of `StandardMiddleware`. Developers can replace this with a custom implementation that adheres to the required interface.

The following sections describe the library’s pluggable components, their interfaces, and example implementations, presented in the order of the "select organisation" flow.

#### SelectOrganisationMiddleware

Is responsible for determining whether an incoming request should be handled by the DfeSignIn::SelectOrganisation library or left to Rack. Only two routes should be handled by the DfeSignIn::SelectOrganisation library, and by default these are called `/select-organisation` and `/select-organisation/callback`. The example implementation validates these requests, along with whether the user is signed in.

| Interface definition                 | Example implementation             |
| ------------------------------------ | ---------------------------------- |
| `/select_organisation_middleware.rb` | `/standard/standard_middleware.rb` |

Configuration:

```ruby
  DfeSignIn::SelectOrganisation.configure do |config|
    config.middleware_factory = -> (config) {
      DfeSignIn::SelectOrganisation::StandardMiddleware.new(config)
    }
  end
```

Replace StandardMiddleware.new(config) with a custom implementation if needed.

#### SelectOrganisationUserFlow

When DfeSignIn::SelectOrganisation::SelectOrganisationMiddleware determines that a request should be handled, it'll call the applicable method within DfeSignIn::SelectOrganisation::SelectOrganisationUserFlow; either `initiate_select` or `handle_callback`.

| Interface definition                | Example implementation            |
| ----------------------------------- | --------------------------------- |
| `/select_organisation_user_flow.rb` | `/standard/standard_user_flow.rb` |

Configuration:

```ruby
  DfeSignIn::SelectOrganisation.configure do |config|
    config.flow_factory = -> (env) {
      DfeSignIn::SelectOrganisation::StandardUserFlow.new(env)
    }
  end
```

Replace StandardUserFlow.new(env) with a custom implementation if needed.

#### TrackingProvider

During `initiate_select` the response back from DfE public API includes a parameter called `requestId` which we recommend is persisted and validated against the `requestId` parameter returned as part of `handle_callback`. In the example application DfeSignIn::SelectOrganisation::TrackingProvider utilises the session_provider to persist this.

| Interface definition    | Example implementation                    |
| ----------------------- | ----------------------------------------- |
| `/tracking_provider.rb` | `/standard/standard_tracking_provider.rb` |

Configuration:

```ruby
  DfeSignIn::SelectOrganisation.configure do |config|
    config.tracking_provider_factory = -> (env, session_provider) {
      DfeSignIn::SelectOrganisation::StandardTrackingProvider.new(session_provider)
    }
  end
```

Replace StandardTrackingProvider.new(session_provider) with a custom implementation if needed.

#### SessionProvider

Provides a means to persist data that might be required across different HTTP requests. The example implementation utilises `rack.session` to provide this, however, you could implement a redis or database backed implemented if required. The example implementation utilises session_provider to store the Organisation details obtained by DfeSignIn::SelectOrganisation::SelectOrganisationEvents.

| Interface definition   | Example implementation                   |
| ---------------------- | ---------------------------------------- |
| `/session_provider.rb` | `/standard/standard_session_provider.rb` |

Configuration:

```ruby
  DfeSignIn::SelectOrganisation.configure do |config|
    config.session_provider_factory = -> (env) {
      DfeSignIn::SelectOrganisation::StandardSessionProvider.new(env)
    }
  end
```

Replace StandardSessionProvider.new(env) with a custom implementation if needed.

#### SelectOrganisationEvents

When a user has finished making their select organisation choice, they are redirected back, which contains a number of query parameters. These parameters can be considered 'events' which need to be handled. For example, the user chose not to select and organisation and clicked cancel. What should happen. Equally, the user might select and organisation, and this needs to be handled.

| Interface definition             | Example implementation         |
| -------------------------------- | ------------------------------ |
| `/select_organisation_events.rb` | `/standard/standard_events.rb` |

Configuration:

```ruby
  DfeSignIn::SelectOrganisation.configure do |config|
    config.events_provider_factory = -> (env, session_provider) {
      DfeSignIn::SelectOrganisation::StandardEvents.new(session_provider)
    }
  end
```

Replace StandardEvents.new(session_provider) with a custom implementation if needed.

#### UserProvider

At various stages within the "select organisation" flow the example implementation needs to know whether the user is signed-in, and what their dsi-user-id is. Therefore, DfeSignIn::SelectOrganisation::UserProvider provides this abstraction. In the example application `rack.session` is used along with assuming the user can be obtained via `@session[:user]["dsi_user_id"]`.

| Interface definition | Example implementation                |
| -------------------- | ------------------------------------- |
| `/user_provider.rb`  | `/standard/standard_user_provider.rb` |

Configuration:

```ruby
  DfeSignIn::SelectOrganisation.configure do |config|
    config.user_factory = -> (env) {
      DfeSignIn::SelectOrganisation::StandardUserProvider.new(env)
    }
  end
```

Replace StandardUserProvider.new(env) with a custom implementation if needed.
