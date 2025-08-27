# Use the official .NET SDK image as the base image for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0@sha256:7fd287cec03e027e39be13fb06ca1347f1338c7a6ab4ad305d94d91852693f70 AS build

ENV PORT=8080
ENV PATH=${PATH}:/home/site/wwwroot

WORKDIR /home/site/wwwroot

ARG PROJECT_NAME

COPY src ./src
COPY *.props ./
COPY *.targets ./
RUN dotnet publish -c Release -o /app "src/$PROJECT_NAME/$PROJECT_NAME.csproj"

# Use the official ASP.NET runtime image for the Azure Linux-based container
FROM mcr.microsoft.com/dotnet/aspnet:8.0-azurelinux3.0@sha256:d0f82f6e7aab87eb7c1e1f39becc83e8e5987b7b16cc6c7f2b56f86dbb32f92e

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
