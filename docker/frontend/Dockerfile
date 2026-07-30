# --- Build frontend assets ---
FROM node:22-alpine@sha256:cb3143549582cc5f74f26f0992cdef4a422b22128cb517f94173a5f910fa4ee7 AS assets

COPY ./frontend /frontend

WORKDIR /frontend

RUN npm install --ignore-scripts && \
    npm run build


# --- Serve with a lightweight non-root version of nginx ---
FROM nginxinc/nginx-unprivileged:1.29-alpine3.22@sha256:3245911073f8a6f8f5257aa08339096f90ef8d5db70f502f8ffeb2b9ecd5ca18

COPY --from=assets /frontend/dist /usr/share/nginx/html/assets
COPY --from=assets /frontend/node_modules/govuk-frontend/dist/govuk/assets /usr/share/nginx/html/assets/assets
COPY --from=assets /frontend/assets /usr/share/nginx/html/assets/assets

COPY docker/frontend/nginx.conf /etc/nginx/templates/default.conf.template
