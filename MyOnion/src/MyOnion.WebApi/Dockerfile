FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/MyOnion.WebApi/MyOnion.WebApi.csproj", "src/MyOnion.WebApi/"]
COPY ["src/MyOnion.Application/MyOnion.Application.csproj", "src/MyOnion.Application/"]
COPY ["src/MyOnion.Domain/MyOnion.Domain.csproj", "src/MyOnion.Domain/"]
COPY ["src/MyOnion.Infrastructure.Persistence/MyOnion.Infrastructure.Persistence.csproj", "src/MyOnion.Infrastructure.Persistence/"]
COPY ["src/MyOnion.Infrastructure.Shared/MyOnion.Infrastructure.Shared.csproj", "src/MyOnion.Infrastructure.Shared/"]

RUN dotnet restore "src/MyOnion.WebApi/MyOnion.WebApi.csproj"

COPY . .
RUN dotnet publish "src/MyOnion.WebApi/MyOnion.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

RUN useradd --create-home --uid 10001 appuser
USER appuser

EXPOSE 8080
EXPOSE 8443

HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 CMD curl -f http://localhost:8080/health || exit 1

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MyOnion.WebApi.dll"]
