# syntax=docker/dockerfile:1
# ─── Build stage ────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["PRN232.Lab1.API/PRN232.Lab1.API.csproj", "PRN232.Lab1.API/"]
COPY ["PRN232.Lab1.Services/PRN232.Lab1.Services.csproj", "PRN232.Lab1.Services/"]
COPY ["PRN232.Lab1.Repositories/PRN232.Lab1.Repositories.csproj", "PRN232.Lab1.Repositories/"]
RUN dotnet restore "PRN232.Lab1.API/PRN232.Lab1.API.csproj"

COPY . .
WORKDIR /src/PRN232.Lab1.API
RUN dotnet publish "PRN232.Lab1.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ─── Runtime stage ──────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "PRN232.Lab1.API.dll"]
