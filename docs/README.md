# Custom templates for docfx

A custom "dsi" template has been created to give the DfE Sign-in Developer Reference documentation the GOV.UK design system look and feel.

## Directory structure

- `external/` - Content and configuration for the external facing documentation website:

  - Guidance that is aimed towards relying parties.

  - Stripped down version of the DfE Sign-in .NET API reference documentation which is designed to assist relying parties when integrating with DfE Sign-in.

- `internal/` - Content and configuration for the internal facing documentation website:

  - Guidance that is aimed towards internal DfE Sign-in development team.

  - Complete version of the DfE Sign-in .NET API reference documentation which is designed to assist the internal DfE Sign-in development team.

- `namespaces/` - Provides documentation content for namespaces.

  - Documentation comments cannot be used to document namespaces within the various .NET projects.

- `templates/Dfe.SignIn.DocfxPlugin/` - A plugin for docfx which post processes output files:

  - `DsiHtmlPostProcessor` - Manipulates generated HTML files to tweak formatting, add GOV.UK design system classes, generate navigation elements, etc.

  - `DsiSearchIndexPostProcessor` - Generates the "search.json" index file for use with [Lunr.js](https://lunrjs.com/).

- `templates/dsi/` - The custom docfx template files.

## Building and previewing the external documentation

To build and serve the external documentation locally:

```pwsh
# run from root of repository /
./scripts/docs/Start-Site external
```

> **Note:** By default the documentation can be viewed on `http://localhost:8085`.

## Building and previewing the internal documentation

To build and serve the internal documentation locally:

```pwsh
# run from root of repository /
./scripts/docs/Start-Site internal
```

> **Note:** By default the documentation can be viewed on `http://localhost:8086`.

## Building and previewing the snapshot testing sample documentation

To build and serve the sample documentation that is used in snapshot testing locally:

```pwsh
# run from root of repository /
./scripts/docs/Start-Site test
```

> **Note:** By default the documentation can be viewed on `http://localhost:8087`.

## Running snapshot tests

```pwsh
# run from root of repository /
dotnet test docs/templates
```

## Updating snapshots

Snapshot tests will fail when intentional changes are made to how pages are rendered.

Verify each of the mismatching snapshots manually:

1. Verify that the HTML output is as expected.
2. Verify that the rendered pages look as expected in a web browser.

If you are satisfied with the changes then rename the applicable `*.received.txt` files to `*.verified.txt`:

```pwsh
# run from root of repository /
./scripts/docs/Confirm-TestSnapshots
```

Commit the updated snapshot files to the repository.
