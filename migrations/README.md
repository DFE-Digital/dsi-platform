# migrations

Entity Framework Core migrations project for all `dsi-platform` database contexts (`DbDirectoriesContext`, `DbOrganisationsContext`, `DbAuditContext`). This project is not application code — it exists solely so `dotnet ef` has a place to scaffold and store migrations, and so CI can build and verify them.

Migrations for each database live in their own folder under `migrations/`, e.g. `migrations/Directories/`.

## Creating a migration

There are two scripts under `scripts/migrations/`, depending on whether this is the very first migration for a context or a subsequent one.

### First migration for a context: `Add-InitialCreate.ps1`

```powershell
./scripts/migrations/Add-InitialCreate.ps1 -Context directories
```

Scaffolds an `InitialCreate` migration by comparing the EF model against an empty database — i.e. `Up()` will contain `CreateTable` calls for every table currently mapped by the context. Only use this once per context, when no prior migration exists.

### Subsequent migrations: `New-Migration.ps1`

```powershell
./scripts/migrations/New-Migration.ps1 -Context directories -MigrationName AddSomeColumn
```

Scaffolds a migration containing only the diff between the EF model and the previous migration.

`-Context` accepts `directories`, `organisations`, or `audit` and maps to the corresponding `Db{Context}Context` and output folder (e.g. `directories` → `DbDirectoriesContext` → `migrations/Directories/`).

## Baselining a migration against a live database — IMPORTANT

Every one of these contexts (`Directories`, `Organisations`, `Audit`) maps to a database that already existed in production before this migrations project did. That means the first-ever migration scaffolded for a context (its `InitialCreate`) is always a **baseline capture** of a schema that already exists live — it has never actually run, and must never be applied to that live database with a naive `dotnet ef database update`. Doing so will fail at best (tables already exist) and risk data loss at worst if a migration's `Down()` is ever invoked by mistake.

Before any migration can be applied for real against a live database for a context, `__EFMigrationsHistory` must be seeded with a row for that context's `InitialCreate` migration **without executing its `Up()`**. Only once that baseline row exists should EF be allowed to apply any migration that comes after it.

In short, per context, the first time this matters:
1. Scaffold `InitialCreate` with `Add-InitialCreate.ps1` (already done for `Directories` — see `migrations/Directories/20260803140253_InitialCreate.cs`).
2. Insert a row into `__EFMigrationsHistory` for that migration's ID on the live database, without running its `Up()`.
3. From then on, use `New-Migration.ps1` for further changes and apply them normally.

Check the top of each `InitialCreate` migration file for a repeat of this warning specific to that context.

## Building and testing

This project is referenced from `dsi-platform.sln` and is built/tested as part of the standard `dotnet build` / `dotnet test` for the solution — there is no separate pipeline for it.
