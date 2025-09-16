# Base runtime image
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj and restore
COPY WorkerPodService.csproj ./
RUN dotnet restore "./WorkerPodService.csproj"

# Copy all source and build
COPY . .
RUN dotnet build "./WorkerPodService.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish
RUN dotnet publish "./WorkerPodService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_NOLOGO=true

ENTRYPOINT ["dotnet", "WorkerPodService.dll"]
