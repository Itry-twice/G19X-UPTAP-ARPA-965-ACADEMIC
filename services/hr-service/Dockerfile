# Build context: ./services
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["hr-service/PluriOne.HrService.csproj", "hr-service/"]
COPY ["PluriOne.Shared/PluriOne.Shared.csproj", "PluriOne.Shared/"]
RUN dotnet restore "hr-service/PluriOne.HrService.csproj"

COPY . .
RUN dotnet publish "hr-service/PluriOne.HrService.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "PluriOne.HrService.dll"]
