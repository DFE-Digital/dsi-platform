# Middleware for .NET applications

The [.NET packages](~/guide/dotnet-packages.md) that we provide include middleware which is compatible with .NET Core and .NET Framework using OWIN. The middleware aims to handle the [select organisation user flow](~/guide/select-organisation/user-flow.md) automatically.

> For information on the middleware implementation, see: [](xref:Dfe.SignIn.PublicApi.Client.SelectOrganisation.StandardSelectOrganisationMiddleware)


## The standard select organisation user flow

The standard select organisation user flow is responsible for:

  - Initiating the select organisation flow using the provided configuration.

  - Tracking the initiated user interaction.

  - Verifying that a callback correspondes with the initiated request.

  - Decoding and triggering the applicable event in the provided [](xref:Dfe.SignIn.PublicApi.Client.SelectOrganisation.ISelectOrganisationEvents) implementation.

The standard select organisation user flow is used by the standard middleware to take the user through the select organisation journey after they authenticate. This happens when the "dsi_user" claim becomes available and an organisation is yet to be selected.

An application can instantiate the [](xref:Dfe.SignIn.PublicApi.Client.SelectOrganisation.StandardSelectOrganisationUserFlow) service and provide configuration and events to prompt a user to select an organisation manually. This is useful for scenarios where either more granular control is needed or where the select organisation feature is being applied to a completely different use case (for example, selecting an organisation for a one-time action).


## Customising the standard user flow events

The standard user flow events are implemented in the concrete type [](xref:Dfe.SignIn.PublicApi.Client.SelectOrganisation.StandardSelectOrganisationEvents) which responds to the various user interactions.

If neccessary the standard user flow events can be subclassed and configured for use with the standard user flow. This allows specific parts to be extended or overridden as needed.


## Allowing a user to switch organisation

This functionality can be enabled and configured in [](xref:Dfe.SignIn.PublicApi.Client.SelectOrganisation.StandardSelectOrganisationUserFlowOptions) by specifying:

  - [](xref:Dfe.SignIn.PublicApi.Client.SelectOrganisation.StandardSelectOrganisationUserFlowOptions.EnableSelectOrganisationRequests) - Set this to `true`.

  - [](xref:Dfe.SignIn.PublicApi.Client.SelectOrganisation.StandardSelectOrganisationUserFlowOptions.SelectOrganisationRequestPath) - Allows the path to be customised (the default path is `"/select-organisation"`).

Once enabled; link the user to the configured endpoint within your application (for example, `"/select-organisation"`) and the middleware will handle the request:

```html
<a href="/select-organisation">Switch organisation</a>
```


## Custom select organisation user flow

A custom select organisation flow can be crafted by:

  - Reusing the standard select organisation user flow.

  - Providing a custom suite of events by implementing [](xref:Dfe.SignIn.PublicApi.Client.SelectOrganisation.ISelectOrganisationEvents).

  - Configuring either a keyed dependency or some sort of factory method to avoid overriding the standard flow that is used by the middleware.
