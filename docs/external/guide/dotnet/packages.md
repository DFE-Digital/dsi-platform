# Packages for .NET applications

We have made NuGet packages available which services can use in .NET applications.


## Dfe.SignIn.PublicApi.Client

Use the following command to install this package:

```pwsh
To be confirmed...
```

The client library for interacting with the DfE Sign-in API:

  - Can be configured using the Microsoft dependency injection abstractions.

  - Provides a [](xref:System.Net.Http.HttpClient) which can be accessed from [](xref:Dfe.SignIn.PublicApi.Client.IPublicApiClient).

  - Provides [middleware](~/guide/dotnet/middleware.md) to simplify integration with the DfE Sign-in "select organisation" service.

The following contract types need to be implemented and configured for your service:

  - [](xref:Dfe.SignIn.PublicApi.Client.SelectOrganisation.ISelectOrganisationRequestTrackingProvider) - Tracks the current user request to select an organisation.

  - [](xref:Dfe.SignIn.PublicApi.Client.SelectOrganisation.IActiveOrganisationProvider) - Manages the active organisation of the user.


## Dfe.SignIn.PublicApi.Client.AspNetCore

Use the following command to install this package:

```pwsh
To be confirmed...
```

This package is optional and adapts the client library to work with the .NET Core middleware mechanism.

This package also provides the following concrete implementations which use ASP.NET Core sessions for select organisation integration:

  - [](xref:Dfe.SignIn.PublicApi.Client.AspNetCore.SelectOrganisationRequestSessionTracking) - Tracks the current user request to select an organisation.

  - [](xref:Dfe.SignIn.PublicApi.Client.AspNetCore.ActiveOrganisationSessionProvider) - Manages the active organisation of the user.


## Dfe.SignIn.PublicApi.Client.Owin

Use the following command to install this package:

```pwsh
To be confirmed...
```

This package is optional and adapts the client library to work with the OWIN middleware mechanism which is intended for use with .NET Framework application.

This package also provides the following concrete implementations which use .NET Framework sessions for select organisation integration:

  - [](xref:Dfe.SignIn.PublicApi.Client.Owin.SelectOrganisationRequestSessionTracking) - Tracks the current user request to select an organisation.

  - [](xref:Dfe.SignIn.PublicApi.Client.Owin.ActiveOrganisationSessionProvider) - Manages the active organisation of the user.
