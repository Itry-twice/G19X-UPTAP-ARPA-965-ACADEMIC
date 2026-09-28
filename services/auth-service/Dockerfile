# Build context: ./services
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["auth-service/PluriOne.AuthService.csproj", "auth-service/"]
COPY ["PluriOne.Shared/PluriOne.Shared.csproj", "PluriOne.Shared/"]
RUN dotnet restore "auth-service/PluriOne.AuthService.csproj"

COPY . .
RUN dotnet publish "auth-service/PluriOne.AuthService.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# curl solo para el healthcheck del orquestador
RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "PluriOne.AuthService.dll"]
