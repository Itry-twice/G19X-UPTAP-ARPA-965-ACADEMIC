# Build context: ./services
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["web/PluriOne.Web.csproj", "web/"]
COPY ["PluriOne.Shared/PluriOne.Shared.csproj", "PluriOne.Shared/"]
RUN dotnet restore "web/PluriOne.Web.csproj"

COPY . .
RUN dotnet publish "web/PluriOne.Web.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# curl (healthcheck) + tzdata (zona horaria) para la Ciudad de México
RUN apt-get update && apt-get install -y --no-install-recommends curl tzdata \
    && rm -rf /var/lib/apt/lists/*

ENV TZ=America/Mexico_City

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "PluriOne.Web.dll"]
