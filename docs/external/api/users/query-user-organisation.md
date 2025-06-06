# Query user organisation

This endpoint can be used to verify that an organisation meets the filtering requirement for a user. Responds with organisation details when filtering requirement is met.


## Endpoint:

```
POST /v2/users/{userId}/organisations/{organisationId}/query
```


## Interactor (.NET):

<pre class="dotnet-type-summary"><div><a href="xref:Dfe.SignIn.Core.Framework.IInteractor`2">IInteractor</a>&lt;
    <a href="xref:Dfe.SignIn.PublicApi.Client.Users.QueryUserOrganisation_PublicApiRequest">QueryUserOrganisation_PublicApiRequest</a>,
    <a href="xref:Dfe.SignIn.PublicApi.Client.Users.QueryUserOrganisation_PublicApiResponse">QueryUserOrganisation_PublicApiResponse</a>
&gt;</div></pre>


## Request headers:

### Authorization (Required)

The JWT token for authorization should be signed using your API secret, which will be provided to you.


## Request body:

**See also:** [](xref:Dfe.SignIn.PublicApi.Client.Users.QueryUserOrganisation_PublicApiRequestBody)

### filter: object (Optional)

Specifies the filtering organisation filtering requirement for the user.

**See also:** [](xref:Dfe.SignIn.Core.ExternalModels.SelectOrganisation.OrganisationFilter)

**Default value:**
```json
{
  "type": "associated",
  "association": "auto",
  "organisationIds": []
}
```

+-----------------------+---------------------------------------------------------------+
| Property              | Summary                                                       |
+=======================+===============================================================+
| `type`                | The type of filtering:                                        |
|                       |                                                               |
|                       | `"associated"`                                                |
|                       | :   Present organisations that are associated with the user.  |
|                       |                                                               |
|                       | `"associatedInclude"`                                         |
|                       | :   Present organisations that are associated with the user   |
|                       |     but only including those that have been explicitly        |
|                       |     specified in `organisationIds`.                           |
|                       |                                                               |
|                       | `"associatedExclude"`                                         |
|                       | :   Present organisations that are associated with the user   |
|                       |     but excluding any that have been explicitly specified in  |
|                       |     `organisationIds`.                                        |
|                       |                                                               |
|                       | `"anyOf"`                                                     |
|                       | :   Present the organisations that have been explicitly       |
|                       |     specified in `organisationIds` regardless of whether the  |
|                       |     user is associated with them.                             |
|                       |                                                               |
|                       | **See also:** [](xref:Dfe.SignIn.Core.ExternalModels.SelectOrganisation.OrganisationFilterType) |
+-----------------------+---------------------------------------------------------------+
| `association`         | The type of association with the user:                        |
|                       |                                                               |
|                       | `"auto"`                                                      |
|                       | :   Behaves as `assignedToUser` when the service is ID-only;  |
|                       |     otherwise, behaves as `assignedToUserForApplication` for  |
|                       |     a role based service.                                     |
|                       |                                                               |
|                       | `"assignedToUser"`                                            |
|                       | :   Present all organisations that are assigned to the user.  |
|                       |                                                               |
|                       | `"assignedToUserForApplication"`                              |
|                       | :   Present organisations that are assigned to the user where |
|                       |     they have permissions for the service.                    |
|                       |                                                               |
|                       | **See also:** [](xref:Dfe.SignIn.Core.ExternalModels.SelectOrganisation.OrganisationFilterAssociation) |
+-----------------------+---------------------------------------------------------------+
| `organisationIds`     | List of organisation IDs (see `type`).                        |
|                       |                                                               |
|                       | **Type:** `string[]`                                          |
+-----------------------+---------------------------------------------------------------+


## Response body:

### userId: string

The unique value that identifies the DfE Sign-in user that the response was intended for.

### organisation: object | null

This contains the up-to-date organisation details when the selection is valid and meets the filtering requirements for the user.

This value is `null` in the following circumstances:

- The user does not exist.

- The organisation does not exist.

- The organisation selection does not meet the filtering criteria.

**Type:** [](xref:Dfe.SignIn.Core.ExternalModels.Organisations.OrganisationDetails)
