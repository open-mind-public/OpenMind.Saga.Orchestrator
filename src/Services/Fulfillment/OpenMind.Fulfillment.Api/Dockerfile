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
COPY ["src/Services/Fulfillment/OpenMind.Fulfillment.Domain/OpenMind.Fulfillment.Domain.csproj", "src/Services/Fulfillment/OpenMind.Fulfillment.Domain/"]
COPY ["src/Services/Fulfillment/OpenMind.Fulfillment.Application/OpenMind.Fulfillment.Application.csproj", "src/Services/Fulfillment/OpenMind.Fulfillment.Application/"]
COPY ["src/Services/Fulfillment/OpenMind.Fulfillment.Infrastructure/OpenMind.Fulfillment.Infrastructure.csproj", "src/Services/Fulfillment/OpenMind.Fulfillment.Infrastructure/"]
COPY ["src/Services/Fulfillment/OpenMind.Fulfillment.Api/OpenMind.Fulfillment.Api.csproj", "src/Services/Fulfillment/OpenMind.Fulfillment.Api/"]

RUN dotnet restore "src/Services/Fulfillment/OpenMind.Fulfillment.Api/OpenMind.Fulfillment.Api.csproj"

COPY . .
WORKDIR "/src/src/Services/Fulfillment/OpenMind.Fulfillment.Api"
RUN dotnet build "OpenMind.Fulfillment.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OpenMind.Fulfillment.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OpenMind.Fulfillment.Api.dll"]
