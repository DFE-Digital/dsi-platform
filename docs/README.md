# Custom templates for docfx

A custom "dsi" template has been created to give the DfE Sign-in .NET reference documentation the GOV.UK design system look and feel.

## Directory structure

- `external/` - Content and configuration for the external facing documentation website:

  * Guidance that is aimed towards relying parties.

  * Stripped down version of the DfE Sign-in .NET API reference documentation which is designed to assist relying parties when integrating with DfE Sign-in.

- `internal/` - Content and configuration for the internal facing documentation website:

  * Guidance that is aimed towards internal DfE Sign-in development team.

  * Complete version of the DfE Sign-in .NET API reference documentation which is designed to assist the internal DfE Sign-in development team.

- `namespaces/` - Provides documentation content for namespaces.

  * Documentation comments cannot be used to document namespaces within the various .NET projects.

- `templates/Dfe.SignIn.DocfxPlugin/` - A plugin for docfx which post processes output files:

  * `DsiHtmlPostProcessor` - Manipulates generated HTML files to tweak formatting, add GOV.UK design system classes, generate navigation elements, etc.

  * `DsiSearchIndexPostProcessor` - Generates the "search.json" index file for use with [Lunr.js](https://lunrjs.com/).

- `templates/dsi/` - The custom docfx template files.

## Updating scripts and styles

Node tooling is required to bundle and minify the scripts and styles.

```pwsh
# run from root of repository /
npm --prefix docs/templates i
```

With the developer tooling installed:

```pwsh
# run from root of repository /
npm --prefix docs/templates run build
```

> **Note:** Scripts and styles can also be built by running `./scripts/docs/Build-CustomTemplate.ps1`.

## Building the Dfe.SignIn.DocfxPlugin project

The following command can be executed from the "docs" directory:

```pwsh
# run from root of repository /
dotnet build docs/templates
```

Upon running this command the plugin DLL files are copied into the "dsi/plugins" directory.

Plugin DLL files are not committed to this repository.

> **Note:** The plugin can also be built by running `./scripts/docs/Build-CustomTemplate.ps1`.

## Building and previewing the internal documentation

To build and serve the internal documentation locally:

```pwsh
# run from root of repository /
./scripts/docs/Preview-InternalSite.ps1
```

> **Note:** By default the documentation can be viewed on `http://localhost:8080`.

## Building and previewing the external documentation

To build and serve the external documentation locally:

```pwsh
# run from root of repository /
./scripts/docs/Preview-ExternalSite.ps1
```

> **Note:** By default the documentation can be viewed on `http://localhost:8080`.

## Building and previewing the snapshot testing sample documentation

To build and serve the sample documentation that is used in snapshot testing locally:

```pwsh
# run from root of repository /
./scripts/docs/Preview-TestSite.ps1
```

> **Note:** By default the documentation can be viewed on `http://localhost:8080`.

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
./scripts/docs/Verify-TestSnapshots.ps1
```

Commit the updated snapshot files to the repository.

## Building documentation from a pipeline

Pipelines can override global metadata properties using the `-m` command line argument.

> See [docfx build](https://dotnet.github.io/docfx/reference/docfx-cli-reference/docfx-build.html) for further information on CLI arguments.

The build pipeline must execute commands from the working directory `/docs/`.

First the custom template styles, scripts and custom plugin must be built using the following command:

```pwsh
# run from root of repository /
./scripts/docs/Build-CustomTemplate.ps1
```

Ensure that tests pass by running the following command:

```pwsh
# run from root of repository /
dotnet test docs/templates
```

The build pipeline builds the **internal** version of the documentation when deploying to the development environments:

```pwsh
# run from root of repository /
./scripts/docs/Prepare-InternalSite.ps1

dotnet docx docs/internal/docfx.json /
    -m _cdnBaseAddress=CDN_BASE_ADDRESS /
    -m _cdnVersion=CDN_VERSION /
    -m _surveyUrl=SURVEY_URL
```

The build pipeline builds the **external** version of the documentation when deploying to non-development environments:

```pwsh
# run from root of repository /
./scripts/docs/Prepare-ExternalSite.ps1

dotnet docx docs/external/docfx.json /
    -m _cdnBaseAddress=CDN_BASE_ADDRESS /
    -m _cdnVersion=CDN_VERSION /
    -m _surveyUrl=SURVEY_URL
```

When the documentation is built docfx produces some output files which are not needed. These can be removed:

```pwsh
# run from root of repository /
./scripts/docs/Remove-UnnecessaryOutputFiles.ps1
```

The `/docs/_site/` directory is then ready to be deployed.
