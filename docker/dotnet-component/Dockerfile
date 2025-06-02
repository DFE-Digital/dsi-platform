# Use the official .NET SDK image as the base image for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

ENV PORT=8080
ENV PATH=${PATH}:/home/site/wwwroot

WORKDIR /home/site/wwwroot

ARG PROC_FILE_NAME

# Build and publish the application to the /out directory in Release mode
RUN dotnet publish -c Release -o /app src/**/$PROC_FILE_NAME.csproj

# Use the official ASP.NET runtime image for the Azure Linux-based container
FROM mcr.microsoft.com/dotnet/aspnet:8.0-azurelinux3.0 AS runtime

ARG PROC_FILE_NAME
ENV FILE_NAME=$PROC_FILE_NAME

WORKDIR /home/site/wwwroot

RUN chmod a+x /home/site/wwwroot/init.sh
RUN sed -i 's/\r$//' /home/site/wwwroot/init.sh

# Copy the published application files from the build stage
COPY --from=build /app .

# Expose the application port (adjust based on your app's requirements)
EXPOSE 8080

# Define the entry point for the container to start the application
ENTRYPOINT ["/home/site/wwwroot/init.sh"]
#ENTRYPOINT ["tail", "-f", "/dev/null"]
