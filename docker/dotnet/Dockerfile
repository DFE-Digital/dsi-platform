# Use the official .NET SDK image as the base image for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0@sha256:3cef19377b2ef2a0171e930a24627677447c3e41b5f2eab84ff4895f1b15d254 AS build

ENV PORT=8080
ENV PATH=${PATH}:/home/site/wwwroot

WORKDIR /home/site/wwwroot

ARG PROJECT_NAME

COPY src ./src
COPY *.props ./
COPY *.targets ./
RUN dotnet publish -c Release -o /app "src/$PROJECT_NAME/$PROJECT_NAME.csproj" /p:AutomatedRun=true

# Use the official ASP.NET runtime image for the Azure Linux-based container
FROM mcr.microsoft.com/dotnet/aspnet:8.0-azurelinux3.0@sha256:f854e3c899e6174fb19820effe614d4b011e029ca97f06b4a90142a93ab96383

ARG PROJECT_NAME
ENV PROJECT_NAME=$PROJECT_NAME

WORKDIR /home/site/wwwroot

COPY docker/dotnet/init.sh init.sh
RUN chmod a+x /home/site/wwwroot/init.sh \
    && sed -i 's/\r$//' /home/site/wwwroot/init.sh

COPY --from=build /app .

EXPOSE 8080

USER $APP_UID

ENTRYPOINT ["/home/site/wwwroot/init.sh"]
