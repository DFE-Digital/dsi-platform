FROM nginx:alpine

ARG HTML_PATH

RUN mv /etc/nginx/nginx.conf /etc/nginx/nginx.conf.orig && { \
    echo 'env CDN_BASE_ADDRESS;'; \
    echo 'env CDN_VERSION;'; \
    echo 'env SURVEY_URL;'; \
    cat /etc/nginx/nginx.conf.orig; \
    } > /etc/nginx/nginx.conf

COPY docker/static-site/default.conf /etc/nginx/conf.d/default.conf

COPY $HTML_PATH /usr/share/nginx/html

USER nonroot
