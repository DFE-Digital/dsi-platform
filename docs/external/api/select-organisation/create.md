# Create select organisation session

This endpoint can be used to initiate a new "select organisation" session.


## Endpoint:

```
POST /v2/select-organisation
```


## Interactor (.NET):

<pre class="dotnet-type-summary"><div><a href="xref:Dfe.SignIn.Core.Framework.IInteractor`2">IInteractor</a>&lt;
    <a href="xref:Dfe.SignIn.PublicApi.Client.SelectOrganisation.CreateSelectOrganisationSession_PublicApiRequest">CreateSelectOrganisationSession_PublicApiRequest</a>,
    <a href="xref:Dfe.SignIn.PublicApi.Client.SelectOrganisation.CreateSelectOrganisationSession_PublicApiResponse">CreateSelectOrganisationSession_PublicApiResponse</a>
&gt;</div></pre>


## Request headers:

### Authorization (Required)

The JWT token for authorization should be signed using your API secret, which will be provided to you.


## Request body:

**See also:** [](xref:Dfe.SignIn.PublicApi.Client.SelectOrganisation.CreateSelectOrganisationSession_PublicApiRequestBody)

### callbackUrl: string (Required)

The URL where the user selection will be reported to.

Note: The callback should verify the user selection by making use of the [Query user organisation](../users/query-user-organisation.md) endpoint.

### userId: string (Required)

The unique value that identifies the DfE Sign-in user.

Note: This can be retrieved from the "dsi_user" claim.

### prompt: string (Required)

Custom prompt message to show in the "select organisation" UI.

**Default value:**
```json
{
  "heading": "Which organisation would you like to use?",
  "hint": "You are associated with more than one organisation. Select one option."
}
```

+---------------------+-----------------------------------------------------------------+
| Property            | Summary                                                         |
+=====================+=================================================================+
| `heading`           | Heading text that is to be shown in the UI.                     |
| **(Required)**      |                                                                 |
|                     | **Type:** `string`                                              |
+---------------------+-----------------------------------------------------------------+
| `hint`              | The grey hint text that is to be shown in the UI.               |
|                     |                                                                 |
|                     | **Type:** `string`                                              |
|                     |                                                                 |
|                     | **Default value:**                                              |
|                     | ```json                                                         |
|                     | "Select one option."                                            |
|                     | ```                                                             |
+---------------------+-----------------------------------------------------------------+

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

### allowCancel: boolean (Optional)

A value indicating if the user can cancel selection.

**Default value:**
```json
true
```


## Response body:

### requestId: string

A unique identifier representing the request to select an organisation.

This value can be used to verify that a callback later in the user journey corresponds to the specific “select organisation” request by comparing with the rid query parameter of the callback.

### hasOptions: boolean

Indicates if there are options for the user to select from.

### url: string

The user can be redirected to this URL to so that they are prompted to select an organiastion.

In the case where the user would have no options; the relying party can choose:

  - not to redirect the user to the url (more efficient).

  - or, to redirect the user to the url which will immediate invoke the callback (easier to integrate).


## Related content

- [Select organisation callback](~/api/select-organisation/callback.md)
