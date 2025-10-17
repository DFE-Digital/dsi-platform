# Use the official .NET SDK image as the base image for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0@sha256:ff8311847c54c04d1a14c488362807997d59b61372da5095a95f89cbcda7f9b7 AS build

WORKDIR /build

ARG PROJECT_NAME

COPY src ./src
COPY *.props ./
COPY *.targets ./
RUN dotnet publish -c Release -o /app "src/$PROJECT_NAME/$PROJECT_NAME.csproj" /p:AutomatedRun=true

# Use the official ASP.NET runtime image for the Azure Linux-based container
FROM mcr.microsoft.com/dotnet/aspnet:8.0-azurelinux3.0@sha256:dcabccd78f2e368c4749ab964695110e4fc86b44b0b7e14158e712a4049fe404

ARG PROJECT_NAME
ENV PROJECT_NAME=$PROJECT_NAME

WORKDIR /app

COPY --from=build /app .
COPY docker/dotnet/init.sh ./init.sh
RUN chmod +x ./init.sh

EXPOSE 8080

USER $APP_UID

ENTRYPOINT ["/app/init.sh"]
