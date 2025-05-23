# Sample applications

We have developed some sample applications which demonstrate how services can integrate with DfE Sign-in.

> **Important:** We have provided some sample applications to provide an initial starting point. As with any code examples, you should always follow your established guidelines for software development, along with system data and security.


## .NET Core 8.x+

The .NET Core sample application integrates with Entra External ID using the [AddMicrosoftIdentityWebApp](https://learn.microsoft.com/en-us/dotnet/api/microsoft.identity.web.microsoftidentitywebappauthenticationbuilderextensions.addmicrosoftidentitywebapp?view=msal-model-dotnet-latest) function from the [Microsoft.Identity.Web](https://learn.microsoft.com/en-us/entra/msal/dotnet/microsoft-identity-web/) package from Microsoft.

The sample application takes advantage of the [DfE Sign-in Public API Client](xref:Dfe.SignIn.PublicApi.Client) NuGet package to integrate with the DfE Sign-in API and "select organisation" services.

Package dependencies are configured using the dependency injection and options configuration abstractions that are provided by Microsoft. This is compatible with the out-of-the-box .NET Core approach for MVC applications.

[For more information view the README.md](https://github.com/DFE-Digital/dsi-sample-integrations/tree/main/dotnet-core) from the sample application on GitHub.


## .NET Framework 4.6.2+

The .NET Framework sample application integrates with Entra External ID using the [OWIN](https://learn.microsoft.com/en-us/aspnet/aspnet/overview/owin-and-katana/getting-started-with-owin-and-katana) package from Microsoft.

The sample application takes advantage of the [DfE Sign-in Public API Client](xref:Dfe.SignIn.PublicApi.Client) NuGet package to integrate with the DfE Sign-in API and "select organisation" services.

Package dependencies are configured using the dependency injection and options configuration abstractions that are provided by Microsoft. This is compatible with various dependency injection packages that are available for .NET Framework applications or alternatively by using the service locator pattern.

[For more information view the README.md](https://github.com/DFE-Digital/dsi-sample-integrations/tree/main/dotnet-framework) from the sample application on GitHub.


## Ruby

The Ruby sample application makes use of the [omniauth_openid_connect](https://github.com/omniauth/omniauth_openid_connect) package to connect to Entra External ID using OpenID.

The sample application uses the REST endpoints of the [DfE Sign-in Public API](~/api/select-organisation/create.md) to integrate with the DfE "select organisation" service.

[For more information view the README.md](https://github.com/DFE-Digital/dsi-sample-integrations/tree/main/ruby) from the sample application on GitHub.
