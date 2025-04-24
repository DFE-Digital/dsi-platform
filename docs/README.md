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

Node tooling is required to bundle and minify the scripts and styles:

```pwsh
npm i
```

With the developer tooling installed:

```pwsh
npm run build
```

## Building the Dfe.SignIn.DocfxPlugin project

The following command can be executed from the "templates" directory:

```pswh
dotnet build
```

Upon running this command the plugin DLL files are copied into the "dsi/plugins" directory.

Plugin DLL files are not committed to this repository.

## Building the internal documentation

To build and serve the internal documentation locally:

```pwsh
dotnet docfx docs/internal/docfx.json --serve
```

By default the documentation can then be viewed on the URL `http://localhost:8080`.

## Building the external documentation

To build and serve the external documentation locally:

```pwsh
dotnet docfx docs/external/docfx.json --serve
```

By default the documentation can then be viewed on the URL `http://localhost:8080`.

## Building documentation from a pipeline

Pipelines can override global metadata properties using the `-m` command line argument.

> See [docfx build](https://dotnet.github.io/docfx/reference/docfx-cli-reference/docfx-build.html) for further information on CLI arguments.

The build pipeline must execute commands from the working directory `/docs/`.

First a custom plugin must be built using the following command:

```pwsh
dotnet build
```

The build pipeline builds the **internal** version of the documentation when deploying to the development environments:

```pwsh
dotnet docx docs/internal/docfx.json /
    -m _cdnBaseAddress=CDN_BASE_ADDRESS /
    -m _cdnVersion=CDN_VERSION /
    -m _surveyUrl=SURVEY_URL
```

The build pipeline builds the **external** version of the documentation when deploying to non-development environments:

```pwsh
dotnet docx docs/external/docfx.json /
    -m _cdnBaseAddress=CDN_BASE_ADDRESS /
    -m _cdnVersion=CDN_VERSION /
    -m _surveyUrl=SURVEY_URL
```

Execute the `RemoveUnnecessaryOutputFiles.ps1` script:

```pwsh
./scripts/RemoveUnnecessaryOutputFiles.ps1
```

The `/docs/_site/` directory is then ready to be deployed.
