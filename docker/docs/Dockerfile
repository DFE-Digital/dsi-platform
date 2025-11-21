# --- Build and process documentation ---
FROM mcr.microsoft.com/dotnet/sdk:8.0@sha256:ff8311847c54c04d1a14c488362807997d59b61372da5095a95f89cbcda7f9b7 AS build

WORKDIR /work

ARG PROJECT_NAME
ARG CDN_BASE_ADDRESS_FORMAT="%s/%s/"

COPY .config ./.config
COPY docs/templates ./docs/templates
COPY docs/appsettings.json ./docs/appsettings.json

RUN dotnet build ./docs/templates -c Release && \
    dotnet tool install docfx --local

COPY $PROJECT_NAME ./$PROJECT_NAME
COPY docs/namespaces ./docs/namespaces

COPY src ./src
COPY *.props *.targets dsi-platform.sln ./

RUN FRONTEND_VERSION=$( \
    grep '"FrontendVersion"' /work/docs/appsettings.json | sed -E 's/.*"FrontendVersion": *"([^"]+)"/\1/' \
    ) && \
    dotnet docfx "./$PROJECT_NAME/docfx.json" \
    -m _cdnBaseAddress=$(printf "$CDN_BASE_ADDRESS_FORMAT" '${{ENV_CDN_BASE_ADDRESS}}' "$FRONTEND_VERSION") \
    -m _surveyUrl='${{ENV_SURVEY_URL}}' && \
    mv "./$PROJECT_NAME/_site" /_site && \
    rm /_site/index.json /_site/manifest.json /_site/xrefmap.yml && \
    rm -f $(find /_site -type f -path '*/toc.json')


# --- Serve with a lightweight non-root version of nginx ---
FROM nginxinc/nginx-unprivileged:1.29-alpine3.22@sha256:3245911073f8a6f8f5257aa08339096f90ef8d5db70f502f8ffeb2b9ecd5ca18

COPY docker/docs/nginx.conf /etc/nginx/templates/default.conf.template

COPY --from=build /_site /usr/share/nginx/html
