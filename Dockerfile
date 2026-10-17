FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY global.json Directory.Build.props .editorconfig ./
COPY src/Brasa.Domain/Brasa.Domain.csproj src/Brasa.Domain/
COPY src/Brasa.Application/Brasa.Application.csproj src/Brasa.Application/
COPY src/Brasa.Infrastructure/Brasa.Infrastructure.csproj src/Brasa.Infrastructure/
COPY src/Brasa.Api/Brasa.Api.csproj src/Brasa.Api/
RUN dotnet restore src/Brasa.Api/Brasa.Api.csproj

COPY src/ src/
RUN dotnet publish src/Brasa.Api/Brasa.Api.csproj --configuration Release --no-restore --output /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080 \
    Storage__Images__Path=/data/images

RUN mkdir -p /data/images && chown "$APP_UID" /data/images
COPY --from=build /app ./

USER $APP_UID
EXPOSE 8080
VOLUME ["/data/images"]

ENTRYPOINT ["dotnet", "Brasa.Api.dll"]
