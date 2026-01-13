FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["src/BuildingBlocks/OpenMind.BuildingBlocks.Domain/OpenMind.BuildingBlocks.Domain.csproj", "src/BuildingBlocks/OpenMind.BuildingBlocks.Domain/"]
COPY ["src/BuildingBlocks/OpenMind.BuildingBlocks.Application/OpenMind.BuildingBlocks.Application.csproj", "src/BuildingBlocks/OpenMind.BuildingBlocks.Application/"]
COPY ["src/BuildingBlocks/OpenMind.BuildingBlocks.Infrastructure/OpenMind.BuildingBlocks.Infrastructure.csproj", "src/BuildingBlocks/OpenMind.BuildingBlocks.Infrastructure/"]
COPY ["src/BuildingBlocks/OpenMind.BuildingBlocks.IntegrationEvents/OpenMind.BuildingBlocks.IntegrationEvents.csproj", "src/BuildingBlocks/OpenMind.BuildingBlocks.IntegrationEvents/"]
COPY ["src/Services/OrderPlacement/OpenMind.OrderPlacement.Domain/OpenMind.OrderPlacement.Domain.csproj", "src/Services/OrderPlacement/OpenMind.OrderPlacement.Domain/"]
COPY ["src/Services/OrderPlacement/OpenMind.OrderPlacement.Application/OpenMind.OrderPlacement.Application.csproj", "src/Services/OrderPlacement/OpenMind.OrderPlacement.Application/"]
COPY ["src/Services/OrderPlacement/OpenMind.OrderPlacement.Infrastructure/OpenMind.OrderPlacement.Infrastructure.csproj", "src/Services/OrderPlacement/OpenMind.OrderPlacement.Infrastructure/"]
COPY ["src/Services/OrderPlacement/OpenMind.OrderPlacement.Api/OpenMind.OrderPlacement.Api.csproj", "src/Services/OrderPlacement/OpenMind.OrderPlacement.Api/"]

RUN dotnet restore "src/Services/OrderPlacement/OpenMind.OrderPlacement.Api/OpenMind.OrderPlacement.Api.csproj"

COPY . .
WORKDIR "/src/src/Services/OrderPlacement/OpenMind.OrderPlacement.Api"
RUN dotnet build "OpenMind.OrderPlacement.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OpenMind.OrderPlacement.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OpenMind.OrderPlacement.Api.dll"]
