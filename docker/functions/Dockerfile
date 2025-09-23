FROM mcr.microsoft.com/dotnet/sdk:8.0@sha256:3cef19377b2ef2a0171e930a24627677447c3e41b5f2eab84ff4895f1b15d254 AS build

ARG PROJECT_NAME

COPY src ./src
COPY *.props ./
COPY *.targets ./

WORKDIR "/src/$PROJECT_NAME"

RUN mkdir -p /home/site/wwwroot && \
    dotnet publish "$PROJECT_NAME.csproj" --output /home/site/wwwroot

# To enable ssh & remote debugging on app service change the base image to the one below
# FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0-appservice
FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0@sha256:af7966e26343946df11c71662537f2652a6bb4fa787d7b8cfa691ba39c8203f4
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

COPY --from=build ["/home/site/wwwroot", "/home/site/wwwroot"]

RUN chown -R "$APP_UID:$APP_UID" /azure-functions-host && \
    chown -R "$APP_UID:$APP_UID" /home

USER $APP_UID
