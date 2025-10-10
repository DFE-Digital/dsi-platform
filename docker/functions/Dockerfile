FROM mcr.microsoft.com/dotnet/sdk:8.0@sha256:ff8311847c54c04d1a14c488362807997d59b61372da5095a95f89cbcda7f9b7 AS build

ARG PROJECT_NAME

COPY src ./src
COPY *.props ./
COPY *.targets ./

WORKDIR "/src/$PROJECT_NAME"

RUN mkdir -p /home/site/wwwroot && \
    dotnet publish "$PROJECT_NAME.csproj" -c Release --output /home/site/wwwroot /p:AutomatedRun=true

# To enable ssh & remote debugging on app service change the base image to the one below
# FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0-appservice
FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0@sha256:6dcf5357fa1d9a475b94fd064dc8e8b725d16b0fee2567be72585a6c73b99e73
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

COPY --from=build ["/home/site/wwwroot", "/home/site/wwwroot"]

RUN chown -R "$APP_UID:$APP_UID" /azure-functions-host && \
    chown -R "$APP_UID:$APP_UID" /home

USER $APP_UID
