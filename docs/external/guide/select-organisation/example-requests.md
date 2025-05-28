# Example select organisation requests

A select organisation request can be initiated for a user with the [Create select organisation session](~/api/select-organisation/create.md) API.


### Prompt a user to select an organisation with the default configuration

The following request should be suitable for most services:

```json
{
  "callbackUrl": "https://some-service/organisation/callback",
  "userId": " 00000000-0000-0000-0000-000000000000"
}
```

This is equivalent to the following request:

```json
{
  "callbackUrl": "https://some-service/organisation/callback",
  "userId": " 00000000-0000-0000-0000-000000000000",
  "filter": {
    "type": "associated",
    "association": "auto"
  }
}
```


### Prompt a user to select any associated organisation

The following request presents the user with all of the organisations that they are associated with regardless of their access:

```json
{
  "callbackUrl": "https://some-service/organisation/callback",
  "userId": " 00000000-0000-0000-0000-000000000000",
  "filter": {
    "type": "associated",
    "association": "assignedToUser"
  }
}
```

> **Note:** This is what most ID-only services would experience when a user has just authenticated.


### Prompt a user to select an associated organisation which has access

The following request presents the user with organisations that they are both associated with and have access to the service with:

```json
{
  "callbackUrl": "https://some-service/organisation/callback",
  "userId": " 00000000-0000-0000-0000-000000000000",
  "filter": {
    "type": "associated",
    "association": "assignedToUserForApplication"
  }
}
```

> **Note:** This is what most role-based services would experience when a user has just authenticated.


### Prompt user to select specific associated organisations

The following request presents the user with an explicit subset of organisations that they are associated with:

```json
{
  "callbackUrl": "https://some-service/organisation/callback",
  "userId": " 00000000-0000-0000-0000-000000000000",
  "filter": {
    "type": "associatedInclude",
    "association": "assignedToUserForApplication",
    "organisationIds": [
      "00000000-0000-0000-0000-000000000001",
      "00000000-0000-0000-0000-000000000002"
    ]
  }
}
```

Similarly, specific organisations can be excluded:

```json
{
  "callbackUrl": "https://some-service/organisation/callback",
  "userId": " 00000000-0000-0000-0000-000000000000",
  "filter": {
    "type": "anyOf",
    "organisationIds": [
      "00000000-0000-0000-0000-000000000001",
      "00000000-0000-0000-0000-000000000002"
    ]
  }
}
```


## Customise prompt text

The following request presents the user with a custom prompt:

```json
{
  "callbackUrl": "https://some-service/organisation/callback",
  "userId": " 00000000-0000-0000-0000-000000000000",
  "prompt": {
    "heading": "Which organisation would you like to contact?",
    "hint": "Select one option." 
  }
}
```


## Specifying whether the "Cancel" button is shown

The following includes the "Cancel" button when the user is making a selection:

```json
{
  "callbackUrl": "https://some-service/organisation/callback",
  "userId": " 00000000-0000-0000-0000-000000000000",
  "allowCancel": true
}
```

This is useful when a user has chosen to switch to a different organisation and then proceeds to change their mind.

If the "Cancel" button is used during the initial authentication flow then it can choose to follow the sign out journey.
