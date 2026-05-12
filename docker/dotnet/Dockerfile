# Use the official .NET SDK image as the base image for building the application
FROM mcr.microsoft.com/dotnet/sdk@sha256:c804b88aaca8d6b76f65998427adec3101dcbbf2e52e705e2509eb81263aab0c AS build

WORKDIR /build

ARG PROJECT_NAME

COPY src ./src
COPY *.props ./
COPY *.targets ./
RUN dotnet publish -c Release -o /app "src/$PROJECT_NAME/$PROJECT_NAME.csproj" /p:AutomatedRun=true

# Use the official ASP.NET runtime image for the Azure Linux-based container
FROM mcr.microsoft.com/dotnet/aspnet@sha256:dca725aa6ebe67b02b394b226cca9e7df1c2cf65ada0f70440bda4198d71a69f

ARG PROJECT_NAME
ENV PROJECT_NAME=$PROJECT_NAME

WORKDIR /app

COPY --from=build /app .
COPY docker/dotnet/init.sh ./init.sh
RUN chmod +x ./init.sh

EXPOSE 8080

USER $APP_UID

ENTRYPOINT ["/app/init.sh"]
