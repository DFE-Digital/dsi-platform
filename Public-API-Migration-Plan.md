# Plan: Migrate Public API Endpoints from Node.js to .NET (FaUAPI)
<!-- Epic: DSI-8671 — Migrate Public API to FaUAPI service -->

## Context

The DSI platform is migrating the `login.dfe.public-api` Node.js service to a .NET replacement called **FaUAPI** (the `Dfe.SignIn.PublicApi` project in dsi-platform). `Web.Help` and `Web.Profile` are already migrated. Two V2 endpoints are live (`/v2/select-organisation` and `/v2/users/{userId}/organisations/{organisationId}/query`).

**Node.js source:** `c:\repo\node\login.dfe.public-api\src\app\`
**Target project:** `c:\repo\node\dsi-platform\src\Dfe.SignIn.PublicApi\`

---

## Architecture Pattern

```
PublicApi endpoint (GET/POST)
  → dispatches via IInteractionDispatcher
  → Core.UseCases interactor (or NodeApi.Client requester, temporary)
  → reads via IUnitOfWork* (EF gateways)
  → returns response record
```

| Layer | Location |
|-------|----------|
| Contracts | `Core.Contracts/PublicApi/` or `Core.Contracts/Access/` |
| Use cases (InternalApi) | `Core.UseCases/PublicApi/` — registered in `InternalApi/Configuration/PublicApiUseCaseExtensions.cs` |
| Use cases (PublicApi-local) | `Core.UseCases/PublicApi/` — registered in `PublicApi/Configuration/*Extensions.cs` |
| NodeApi.Client (temporary) | `NodeApi.Client/Access/`, `NodeApi.Client/Organisations/` etc. |
| Endpoint | `PublicApi/Endpoints/{Domain}/` |
| Tests | `tests/Dfe.SignIn.PublicApi.UnitTests/` |

---

## Migration Tickets

### ✅ DSI-8678 — Get user access to service
**Endpoint:** `GET /services/{sid}/organisations/{oid}/users/{uid}`
**Node.js:** `src/app/services/getUsersAccess.js`
**Status: IMPLEMENTED**

Returns a user's roles, service identifiers, and legacy IDs for a given service+organisation. Returns 404 if the user has no access or the organisation does not exist.

**Files created:**
- `Core.Contracts/Access/GetUserServiceAccess.cs` — `GetUserServiceAccessRequest/Response`, `UserServiceAccess`, `UserServiceRole`, `UserServiceIdentifier`
- `Core.Contracts/Access/UserServiceAccessNotFoundException.cs`
- `Core.Contracts/PublicApi/GetUserOrganisationIdentifiers.cs`
- `Core.Contracts/PublicApi/GetUserAccessToService.cs` — response shape matches Node.js exactly
- `NodeApi.Client/Access/GetUserServiceAccessNodeRequester.cs` — calls Access API: `GET /users/{uid}/services/{sid}/organisations/{oid}`
- `NodeApi.Client/Access/Models/UserServiceAccessDto.cs`
- `Core.UseCases/PublicApi/GetUserOrganisationIdentifiersUseCase.cs` — EF read from `UserOrganisationEntity`
- `Core.UseCases/PublicApi/GetUserAccessToServiceUseCase.cs` — coordinates 3 calls in parallel
- `PublicApi/Endpoints/Services/ServiceEndpoints.cs`
- `PublicApi/Endpoints/Services/GetUserAccessToService.cs`
- `PublicApi/Configuration/ServiceEndpointExtensions.cs`

**Files modified:**
- `Core.Contracts/Organisations/Organisation.cs` — added `LegacyId`
- `Core.UseCases/Organisations/OrganisationHelpers.cs` — maps `LegacyId`
- `InternalApi/Configuration/PublicApiUseCaseExtensions.cs` — registers `GetUserOrganisationIdentifiersUseCase`
- `PublicApi/Program.cs` — wires up `SetupServiceInteractions()` and `UseServiceEndpoints()`

---

### ✅ DSI-8679 — Get organisations for user
**Endpoint:** `GET /users/{id}/organisations`
**Node.js:** `src/app/users/getUsersOrganisations.js`
**Status: IMPLEMENTED**

Returns the list of organisations a user belongs to. Filters out organisations with `status.id = 0` (hidden id-only orgs). Returns 404 if none remain after filtering.

**Data source:** Organisations Node API via `GetOrganisationsAssociatedWithUserNodeRequester` (already exists in `NodeApi.Client`)

**Files created:**
- `Core.Contracts/PublicApi/GetUserOrganisations.cs` — `GetUserOrganisationsRequest/Response`
- `Core.UseCases/PublicApi/GetUserOrganisationsUseCase.cs` — dispatches to Orgs Node API, filters `OrganisationStatus.Hidden`, throws `UserNotFoundException` if empty
- `PublicApi/Endpoints/Users/GetUserOrganisations.cs` — endpoint handler
- `PublicApi/Configuration/UserEndpointExtensions.cs` — registers `GetUserOrganisationsUseCase`

**Files modified:**
- `PublicApi/Endpoints/Users/UserEndpoints.cs` — added `GET users/{userId}/organisations` route
- `PublicApi/Program.cs` — added `builder.Services.SetupUserInteractions()`

---

### DSI-8681 — Get organisations for user (V2 — includes Provider Profile attributes)
**Endpoint:** `GET /users/{id}/v2/organisations`
**Node.js:** `src/app/users/getUsersOrganisationsV2.js`

Currently identical logic to V1 in Node.js, but the ticket name indicates V2 should surface additional Provider Profile organisation attributes (ProviderProfileId, UPIN, PIMSStatus, GIASStatus, etc.) that V1 omits. **Confirm the exact extra fields required with TA before implementing.**

**Implementation steps:**
1. Determine from the ticket/TA which additional fields are required for V2
2. Define `GetUserOrganisationsV2Response` in `Core.Contracts/PublicApi/` with extended fields
3. Implement `GetUserOrganisationsV2UseCase` — same filter logic as V1, different response shape
4. Add `GET users/{userId}/v2/organisations` endpoint — note: the existing `POST v2/users/{userId}/organisations/{organisationId}/query` is a separate endpoint

---

### DSI-8680 — Get roles for service
**Endpoint:** `GET /services/{clientId}/roles`
**Node.js:** `src/app/services/getServiceRoles.js`

Returns all roles for a service. **Authorisation check:** the calling client must be either the same service or its parent (`client.id === service.parentId`). Returns 403 if not authorised; 404 if service not found.

**Data source:** `RoleEntity` (Organisations DB via EF)

**Implementation steps:**
1. Define `GetServiceRolesRequest` / `GetServiceRolesResponse` in `Core.Contracts/PublicApi/`
2. Implement `GetServiceRolesUseCase` — look up service by `clientId` via Applications NodeApi, check parent/self authorisation against `IClientSession.ClientId`, return roles for the service
3. Add `GET services/{clientId}/roles` endpoint — 403 for auth failure, 404 for not found
4. Tests: authorised same-service, authorised as parent, 403 case

---

### DSI-8673 — Invite User
**Endpoint:** `POST /services/{sid}/invitations`
**Node.js:** `src/app/services/inviteUser.js`

Validates the invitation payload then queues an invitation job. Returns 202 Accepted (no body).

**Required fields:** `sourceId`, `given_name`, `family_name`, `email` (valid format)
**Optional fields:** `organisation`, `callback` (valid HTTP/HTTPS URI), `userRedirect` (valid URI, defaults to service's first redirect URI), `inviteSubjectOverride`, `inviteBodyOverride`

**Note:** Node.js queues via `jobsClient.sendInvitationRequest()`. Confirm the equivalent job type in `Gateways.ServiceBus` or `login.dfe.jobs` before implementing — this ticket may depend on a separate jobs infrastructure ticket.

**Implementation steps:**
1. Define `InviteUserToServiceRequest` / `InviteUserToServiceResponse` in `Core.Contracts/PublicApi/` with `[Required]` / `[EmailAddress]` / `[Url]` validation annotations
2. Implement `InviteUserToServiceUseCase` — verify service exists, dispatch job via ServiceBus
3. Add `POST services/{serviceId}/invitations` endpoint returning 202
4. Tests: validation errors (missing fields, bad URI, bad email), 404 service not found, 202 success

---

### DSI-8683 — Service Users without filters
**Endpoint:** `GET /users/` (no `status`/`from`/`to` query params present)
**Node.js:** `src/app/users/getServiceUsers.js` → `listUsersWithOutFilters`

Paginated list of users for the calling service. Required params: `page`, `pageSize`. Returns user details, organisation, and roles per user.

**Data sources:** `getFilteredServiceUsersRaw` (page of user-service records) + `getUsersRaw` (user names/email) + `getServiceUsersPostRaw` (roles per user per org)

**Implementation steps:**
1. Define `ListServiceUsersRequest` (page, pageSize) / `ListServiceUsersResponse` in `Core.Contracts/PublicApi/`
2. Implement `ListServiceUsersUseCase` — paginated query, join user details and roles
3. Add `GET users/` endpoint — 400 if page/pageSize are missing or invalid

---

### DSI-8684 — Service Users with filters
**Endpoint:** `GET /users/` (with `status`, `from`, and/or `to` query params)
**Node.js:** `src/app/users/getServiceUsers.js` → `listUsersWithFilters`

Same as DSI-8683 but with optional filters. Additional validations:
- `status` must be `"0"` or `"1"` if provided
- `from` / `to` must be valid ISO dates if provided
- Date range must not be in the future
- `from` must not be after `to`
- Maximum 90 days between dates
- If only one date is given, the other is inferred at ±90 days and a `warning` field is added to the response
- If neither date is given (but another filter is), defaults to last 90 days with `warning`
- Response includes `dateRange` string when both dates are present
- Response includes `warning` string when dates are inferred or capped

**Implementation steps:**
1. Extend `ListServiceUsersRequest` with optional `status`, `from`, `to` params
2. Implement date range inference and validation in the use case
3. Reuse the same endpoint as DSI-8683 — dispatch to filtered variant when any filter param is present
4. Tests: all validation error paths, date range inference, warning messages

---

### DSI-8685 — Approvers for organisations
**Endpoint:** `GET /users/approvers`
**Node.js:** `src/app/users/getApprovers.js`

Paginated list of approvers (org role ID = 10000) for the calling service. **Requires** `canViewApproverReport === "true"` in the calling client's relying party params — returns 403 if not set.

**Note:** `req.client.relyingParty.params.canViewApproverReport` in Node.js needs to be replicated via `IClientSession` in .NET. Confirm where this flag is stored (it is a service param, likely in the Applications/Organisations DB).

**Implementation steps:**
1. Define `GetApproversRequest` (page, pageSize) / `GetApproversResponse` in `Core.Contracts/PublicApi/`
2. Implement `GetApproversUseCase` — check `canViewApproverReport` permission via `IClientSession`, query `UserOrganisationEntity` where `RoleId == 10000` for the service, join user details
3. Add `GET users/approvers` endpoint — 403 if permission missing

---

### DSI-8686 — Get organisation users by roles using UKPRN
**Endpoint:** `GET /organisations/{id}/users` (UKPRN lookup path)
**Node.js:** `src/app/organisations/getUsersByRoles.js` (UKPRN branch)

Given an identifier value, looks up matching organisations by **UKPRN**. Returns users for those organisations, optionally filtered by `roles` (comma-separated role codes), `email`, and `userId`. Returns `{ ukprn: id, users: [...] }`.

**Implementation steps:**
1. Define `GetOrganisationUsersByUkprnRequest` (id, roles?, email?, userId?) / `GetOrganisationUsersByUkprnResponse` in `Core.Contracts/PublicApi/`
2. Implement `GetOrganisationUsersByUkprnUseCase` — query `OrganisationEntity` by `Ukprn`, fetch service users per org, join user details, apply filters
3. Add `GET organisations/{id}/users` endpoint — also handles UPIN fallback (see DSI-8688)

---

### DSI-8688 — Get organisation users by roles using UPIN
**Endpoint:** `GET /organisations/{id}/users` (UPIN fallback path)
**Node.js:** `src/app/organisations/getUsersByRoles.js` (UPIN branch — only reached when UKPRN lookup returns empty)

Same logic as DSI-8686 but uses **UPIN** for the org lookup. Response shape: `{ upin: id, users: [...] }`.

**Note:** This is the fallback in a single endpoint — if UKPRN finds no matches, try UPIN. Implement after DSI-8686 is done.

---

### DSI-8687 — Retrieve Organisation Users by Filtered Criteria
**Endpoint:** `GET /organisations/{id}/users` (filtering + deduplication logic)
**Node.js:** `src/app/organisations/getUsersByRoles.js` (filtering + dedup)

Filtering on top of the UKPRN/UPIN lookup:
- `roles` — comma-separated role codes; only return users with at least one matching role
- `email` — case-insensitive exact match
- `userId` — case-insensitive exact match
- **Deduplication:** users appearing across multiple orgs (same UKPRN/UPIN) are merged — role arrays are unioned by set

**Implementation steps:**
1. Implement role-code filtering, email filter, userId filter, and email-based dedup + role union logic within the use cases from DSI-8686/8688
2. Tests: role filtering, email filter, userId filter, deduplication across multiple org matches

---

### DSI-8689 — Retrieve Organisation Users by Filtered Criteria (extended)
**Endpoint:** `GET /organisations/{id}/users`

Likely extends DSI-8687. **Confirm scope with TA** — this may cover scenarios not addressed in DSI-8687 (e.g. combined UKPRN+UPIN results, additional query parameters, or a variant of the endpoint).

---

### DSI-8682 — Get organisations and services for user
**Endpoint:** `GET /users/{id}/organisationservices`
**Node.js:** `src/app/users/getUsersOrganisationsAndServices.js`

The most data-intensive endpoint. Returns full user profile + list of organisations + for each org: all services the user has access to + roles per service. Includes org category/status lookups and all legacy ID fields.

**Data sources:** user details, user's service organisations, org categories/statuses, services per org, service details, roles per service

**Implementation steps:**
1. Define `GetUserOrganisationsAndServicesRequest` / `GetUserOrganisationsAndServicesResponse` (large response type — refer to Node.js handler for complete field list) in `Core.Contracts/PublicApi/`
2. Implement `GetUserOrganisationsAndServicesUseCase` — break into sub-queries to keep testable
3. Add `GET users/{userId}/organisationservices` endpoint
4. Tests: test each data join individually

---

## Non-Migration Tickets

| Ticket | Title | Notes |
|--------|-------|-------|
| DSI-8672 | Create an Open API Specification for the DSI Public API | Can be auto-generated from the existing Swagger config in `PublicApi/Program.cs` once all endpoints are implemented |
| DSI-8674 | Onboarding Process Documentation for FaUAPI | Document how API consumers authenticate and call FaUAPI; outputs from DSI-8693 |
| DSI-8675 | Register DfE Sign-In Public API in FaUAPI for all environments | DevOps: register service in Dev, Test, Pre-Prod, Prod |
| DSI-8676 | Update onboarding process for API consumers to use FaUAPI | Update existing docs/portals to point at FaUAPI instead of Node API |
| DSI-8691 | Management of Public API in FaUAPI | Admin/management tooling for the FaUAPI service |
| DSI-8692 | Monitoring Usage of the Public API in FaUAPI | Metrics, dashboards, alerting — OpenTelemetry → Azure Monitor is already configured in `Program.cs` |
| DSI-8693 | Spike: Investigate and define onboarding process for API consumers to use FaUAPI | Research spike; outputs feed into DSI-8674 and DSI-8676 |

---

## Suggested Implementation Order

| Order | Ticket | Rationale |
|-------|--------|-----------|
| ✅ 1 | DSI-8678 — Get user access to service | Done — establishes patterns |
| ✅ 2 | DSI-8679 — Get organisations for user | Done |
| 3 | DSI-8681 — Get organisations for user V2 | Builds on 8679; clarify extra fields first |
| 4 | DSI-8680 — Get roles for service | Straightforward read + new auth check pattern |
| 5 | DSI-8683 — Service Users without filters | Establishes pagination pattern |
| 6 | DSI-8684 — Service Users with filters | Builds on 8683; date logic is the complexity |
| 7 | DSI-8685 — Approvers | Same pagination pattern + permission check |
| 8 | DSI-8686 — Org users by UKPRN | First half of org users endpoint |
| 9 | DSI-8688 — Org users by UPIN | Second half (fallback) |
| 10 | DSI-8687 — Org users filtered criteria | Filtering + dedup on top of 8686/8688 |
| 11 | DSI-8689 — Org users filtered criteria (extended) | Clarify scope with TA first |
| 12 | DSI-8682 — Get organisations and services for user | Most data-intensive — do last |
| 13 | DSI-8673 — Invite User | Depends on jobs/ServiceBus infra confirmation |

---

## Acceptance Criteria Template (per ticket)

- **AC1:** Endpoint rewritten in .NET with feature parity
- **AC2:** All existing test packs pass (Node.js tests ported to xUnit)
- **AC3:** Code comments document internal logic (data joins, fallback behaviour, edge cases)
- **AC4:** Confluence page documents endpoint purpose, auth requirements, request/response shape
- **AC5:** Migrated code and documentation reviewed by another developer and TA

---

## Verification Steps (per endpoint)

1. `dotnet test tests/Dfe.SignIn.PublicApi.UnitTests/`
2. `dotnet test` (solution-wide)
3. Run PublicApi locally with a test Bearer token and call the endpoint
4. Compare response shape against Node.js running side-by-side
5. Confirm `PublicApi.Client` has a corresponding method that serialises/deserialises correctly
