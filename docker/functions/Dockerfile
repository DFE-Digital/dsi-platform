FROM mcr.microsoft.com/dotnet/sdk@sha256:c804b88aaca8d6b76f65998427adec3101dcbbf2e52e705e2509eb81263aab0c AS build

ARG PROJECT_NAME

COPY src ./src
COPY *.props ./
COPY *.targets ./

WORKDIR "/src/$PROJECT_NAME"

RUN mkdir -p /home/site/wwwroot && \
    dotnet publish "$PROJECT_NAME.csproj" -c Release --output /home/site/wwwroot /p:AutomatedRun=true

# To enable ssh & remote debugging on app service change the base image to the one below
# FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated10.0-appservice
FROM mcr.microsoft.com/azure-functions/dotnet-isolated@sha256:af634f8ded5c98434d5f832de457fcc5f42738439ac55510a4070e2aa1a24802
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

COPY --from=build ["/home/site/wwwroot", "/home/site/wwwroot"]

RUN chown -R "$APP_UID:$APP_UID" /azure-functions-host && \
    chown -R "$APP_UID:$APP_UID" /home

USER $APP_UID
