# --- Build frontend assets ---
FROM node:22-alpine AS assets

COPY ./docs/templates /templates

WORKDIR /templates

RUN npm install --ignore-scripts && \
    npm run build


# --- Build and process documentation ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /work

ARG PROJECT_NAME

COPY .config ./.config
COPY docs/templates ./docs/templates
COPY --from=assets /templates/dsi/dist ./docs/templates/dsi/dist

RUN dotnet build ./docs/templates -c Release && \
    dotnet tool install docfx --local

COPY $PROJECT_NAME ./$PROJECT_NAME
COPY docs/namespaces ./docs/namespaces

COPY src ./src
COPY *.props *.targets dsi-platform.sln ./

RUN dotnet docfx "./$PROJECT_NAME/docfx.json" \
    -m _cdnBaseAddress='${{ENV_CDN_BASE_ADDRESS}}' \
    -m _cdnVersion='${{ENV_CDN_VERSION}}' \
    -m _surveyUrl='${{ENV_SURVEY_URL}}' && \
    mv "./$PROJECT_NAME/_site" /_site && \
    rm /_site/index.json /_site/manifest.json /_site/xrefmap.yml && \
    rm -f $(find /_site -type f -path '*/toc.json')


# --- Serve with a lightweight non-root version of nginx ---
FROM nginxinc/nginx-unprivileged:1.27.5-alpine3.21

COPY docker/docs/nginx.conf /etc/nginx/templates/default.conf.template

COPY --from=build /_site /usr/share/nginx/html
