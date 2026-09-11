# syntax=docker/dockerfile:1

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /source
COPY . /source

WORKDIR /source/FauluApartmentAPI

ARG TARGETARCH

RUN if [ "$TARGETARCH" = "amd64" ]; then ARCH=x64; else ARCH=$TARGETARCH; fi && \
    dotnet publish -a $ARCH \
    --self-contained false \
    -o /app

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

WORKDIR /app

COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "FauluApartmentAPI.dll"]