# Overview of select organisation user flow

This page presents a high level overview of the user flow that occurs from initiating the prompt to select an organisation; through to reacting to that user selection.

![Diagram showing the select organisation user flow through system components](~/assets/images/select-organisation-user-flow.png)


## 1. Initiate prompt to select an organisation

A service can use the [Create select organisation session](~/api/select-organisation/create.md) API to create a new "select organisation" session. The way in which organisations are filtered can be specified when making this request.

The API responds indicating whether or not the user can make a selection. If there are no options for the user; then the service does not need to redirect the user.


## 2. Redirect the user to the provided URL

The user will then be presented with the list of organisations that they can choose from.

> **Note:** If there are no organisations for the user to choose from then the user will be redirected to the callback with an error code.


## 3. The user submits their selection

> **Note:** If there is only one organisation to choose from then that organisation will be selected automatically.

The "select organisation" session is verified by the "select organisation" service.

If a problem has occurred then the user may be presented with an error:

  - They left it too long and the session has expired.

  - The URL has been tampered with.

  - The user tried to select an organisation that was not presented to them.


## 4. Redirect user to service callback handler

The [select organisation callback](~/api/select-organisation/callback.md) of the relying party application can respond to the following user interactions:

  - The user has selected an organisation.

  - The user selected the "Cancel" button.

  - The user requested to sign out of the service.

  - An error has occurred.

The service callback handler should not immediately trust the user selection.


## 5. Query the selected organisation for the user

The user selection can be verified using the [Query user organisation](~/api/users/query-user-organisation.md) API.

This endpoint will only return the organisation details if the user selection was, or is still, valid.

A service can reuse this API to refresh the organisation details for the user as and when necessary.
