FROM nginxinc/nginx-unprivileged:1.27.5-alpine3.21

ARG HTML_PATH

COPY docker/static-site/nginx.conf /etc/nginx/templates/default.conf.template

COPY $HTML_PATH /usr/share/nginx/html
