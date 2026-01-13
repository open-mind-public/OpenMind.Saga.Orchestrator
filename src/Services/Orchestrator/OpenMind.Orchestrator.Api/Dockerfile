FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy solution and project files
COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["src/BuildingBlocks/OpenMind.BuildingBlocks.Domain/OpenMind.BuildingBlocks.Domain.csproj", "src/BuildingBlocks/OpenMind.BuildingBlocks.Domain/"]
COPY ["src/BuildingBlocks/OpenMind.BuildingBlocks.Application/OpenMind.BuildingBlocks.Application.csproj", "src/BuildingBlocks/OpenMind.BuildingBlocks.Application/"]
COPY ["src/BuildingBlocks/OpenMind.BuildingBlocks.Infrastructure/OpenMind.BuildingBlocks.Infrastructure.csproj", "src/BuildingBlocks/OpenMind.BuildingBlocks.Infrastructure/"]
COPY ["src/BuildingBlocks/OpenMind.BuildingBlocks.IntegrationEvents/OpenMind.BuildingBlocks.IntegrationEvents.csproj", "src/BuildingBlocks/OpenMind.BuildingBlocks.IntegrationEvents/"]
COPY ["src/Services/Orchestrator/OpenMind.Orchestrator.Api/OpenMind.Orchestrator.Api.csproj", "src/Services/Orchestrator/OpenMind.Orchestrator.Api/"]

RUN dotnet restore "src/Services/Orchestrator/OpenMind.Orchestrator.Api/OpenMind.Orchestrator.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/Services/Orchestrator/OpenMind.Orchestrator.Api"
RUN dotnet build "OpenMind.Orchestrator.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OpenMind.Orchestrator.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OpenMind.Orchestrator.Api.dll"]
