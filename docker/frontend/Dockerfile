# --- Build frontend assets ---
FROM node:22-alpine@sha256:1b2479dd35a99687d6638f5976fd235e26c5b37e8122f786fcd5fe231d63de5b AS assets

COPY ./frontend /frontend

WORKDIR /frontend

RUN npm install --ignore-scripts && \
    npm run build


# --- Serve with a lightweight non-root version of nginx ---
FROM nginxinc/nginx-unprivileged:1.29-alpine3.22@sha256:7f2bd114a80058e93237bc9ba8f8febff89f918ff344c707d78b295027256fd1

COPY --from=assets /frontend/dist /usr/share/nginx/html
COPY --from=assets /frontend/node_modules/govuk-frontend/dist/govuk/assets /usr/share/nginx/html/assets
COPY --from=assets /frontend/assets /usr/share/nginx/html/assets

COPY docker/frontend/nginx.conf /etc/nginx/templates/default.conf.template
