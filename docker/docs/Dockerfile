# --- Build and process documentation ---
FROM mcr.microsoft.com/dotnet/sdk:8.0@sha256:7fd287cec03e027e39be13fb06ca1347f1338c7a6ab4ad305d94d91852693f70 AS build

WORKDIR /work

ARG PROJECT_NAME

COPY .config ./.config
COPY docs/templates ./docs/templates

RUN dotnet build ./docs/templates -c Release && \
    dotnet tool install docfx --local

COPY $PROJECT_NAME ./$PROJECT_NAME
COPY docs/namespaces ./docs/namespaces

COPY src ./src
COPY *.props *.targets dsi-platform.sln ./

RUN dotnet docfx "./$PROJECT_NAME/docfx.json" \
    -m _cdnBaseAddress='${{ENV_CDN_BASE_ADDRESS}}' \
    -m _surveyUrl='${{ENV_SURVEY_URL}}' && \
    mv "./$PROJECT_NAME/_site" /_site && \
    rm /_site/index.json /_site/manifest.json /_site/xrefmap.yml && \
    rm -f $(find /_site -type f -path '*/toc.json')


# --- Serve with a lightweight non-root version of nginx ---
FROM nginxinc/nginx-unprivileged:1.29-alpine3.22@sha256:7f2bd114a80058e93237bc9ba8f8febff89f918ff344c707d78b295027256fd1

COPY docker/docs/nginx.conf /etc/nginx/templates/default.conf.template

COPY --from=build /_site /usr/share/nginx/html
