# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
# For more information, please see https://aka.ms/containercompat

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0.11-bullseye-slim AS base
WORKDIR /app
EXPOSE 8080

# Listen on port 8080 inside the container
ENV ASPNETCORE_URLS=http://+:8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0.11-bullseye-slim AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["FauluApartmentAPI/FauluApartmentAPI.csproj", "FauluApartmentAPI/"]
RUN dotnet restore "FauluApartmentAPI/FauluApartmentAPI.csproj"
COPY . .
WORKDIR "/src/FauluApartmentAPI"
RUN dotnet build "FauluApartmentAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
# Produce a self-contained Linux x64 publish so the required .NET runtime is bundled
RUN dotnet publish "FauluApartmentAPI.csproj" -c $BUILD_CONFIGURATION -r linux-x64 --self-contained true -o /app/publish /p:PublishTrimmed=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM mcr.microsoft.com/dotnet/runtime-deps:bullseye-slim AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Ensure the app host is executable and run the self-contained app
RUN if [ -f ./FauluApartmentAPI ]; then chmod +x ./FauluApartmentAPI || true; fi
ENTRYPOINT ["./FauluApartmentAPI"]
