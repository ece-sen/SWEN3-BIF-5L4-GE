# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Paperless.BatchProcess/Paperless.BatchProcess.csproj", "Paperless.BatchProcess/"]
COPY ["Paperless.DAL/Paperless.DAL.csproj", "Paperless.DAL/"]
COPY ["Paperless.Models/Paperless.Models.csproj", "Paperless.Models/"]
RUN dotnet restore "./Paperless.BatchProcess/Paperless.BatchProcess.csproj"
COPY . .
WORKDIR "/src/Paperless.BatchProcess"
RUN dotnet build "./Paperless.BatchProcess.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Paperless.BatchProcess.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
RUN apt-get update && apt-get install -y cron
WORKDIR /app
COPY --from=publish /app/publish .
RUN echo "0 1 * * * dotnet /app/Paperless.BatchProcess.dll | tee -a /var/log/batch.log >> /proc/1/fd/1 2>&1" \
    > /etc/cron.d/batch-cron
RUN crontab /etc/cron.d/batch-cron
RUN touch /var/log/batch.log
CMD ["cron", "-f"]