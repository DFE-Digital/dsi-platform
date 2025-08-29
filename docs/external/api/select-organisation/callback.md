# Select organisation callback

The user is redirected to a callback URL within the relying party application when a user organisation selection has been processed allowing the application to react accordingly.

> **Important:** The service callback handler should not immediately trust the user selection. The user selection can be verified using the [Query user organisation](~/api/users/query-user-organisation.md) API.

## Query parameters

### type: string

A value indicating the type of callback which will be one of the following values:

<!-- prettier-ignore-start -->
+---------------------+-----------------------------------------------------------------+
| Value               | Summary                                                         |
+=====================+=================================================================+
| `selection`         | The user has made a selection.                                  |
+---------------------+-----------------------------------------------------------------+
| `cancel`            | The user has requested to cancel the “select organisation”      |
|                     | journey. This is applicable where allowCancel was true.         |
+---------------------+-----------------------------------------------------------------+
| `signOut`           | The user has requested to sign out.                             |
+---------------------+-----------------------------------------------------------------+
| `error`             | An error has occurred.                                          |
+---------------------+-----------------------------------------------------------------+
<!-- prettier-ignore-end -->

This parameter is always present.

### rid: string

The unique identifier associated with the request which can be used to:

- Ensure that the callback corresponds to the request that is expected.

- That the callback cannot be replayed.

This parameter is always present.

### id: string

The unique identifier of the selected organisation when `type` is `selection`.

The relying party must not trust the value of this parameter.

The value of this parameter can be verified using the [Query user organisation](~/api/users/query-user-organisation.md) API endpoint to ensure that the selection meets the filtering requirements.

### code: string

The error code when `type` is `error`.

This parameter can have one of the following values:

+---------------------+-----------------------------------------------------------------+
| Value | Summary |
+=====================+=================================================================+
| `internalError` | An unexpected internal error has occurred. |
+---------------------+-----------------------------------------------------------------+
| `invalidSelection` | An invalid selection was made. |
+---------------------+-----------------------------------------------------------------+
| `noOptions` | There were no options for the user to choose from. |
+---------------------+-----------------------------------------------------------------+

**See also:** [](xref:Dfe.SignIn.Core.ExternalModels.SelectOrganisation.SelectOrganisationErrorCode)
