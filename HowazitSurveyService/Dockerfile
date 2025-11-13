# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project file first for better layer caching
COPY ["HowazitSurveyService.csproj", "."]

# Restore dependencies with BuildKit cache mount (faster NuGet restores)
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore "./HowazitSurveyService.csproj" --verbosity quiet

# Copy source code (this layer invalidates when source changes)
COPY . .

# Build the project (restore again to ensure packages are available, but use cache)
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet build "./HowazitSurveyService.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish "./HowazitSurveyService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HowazitSurveyService.dll"]