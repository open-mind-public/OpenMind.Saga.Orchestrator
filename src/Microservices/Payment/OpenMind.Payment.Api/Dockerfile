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
COPY ["src/Services/Payment/OpenMind.Payment.Domain/OpenMind.Payment.Domain.csproj", "src/Services/Payment/OpenMind.Payment.Domain/"]
COPY ["src/Services/Payment/OpenMind.Payment.Application/OpenMind.Payment.Application.csproj", "src/Services/Payment/OpenMind.Payment.Application/"]
COPY ["src/Services/Payment/OpenMind.Payment.Infrastructure/OpenMind.Payment.Infrastructure.csproj", "src/Services/Payment/OpenMind.Payment.Infrastructure/"]
COPY ["src/Services/Payment/OpenMind.Payment.Api/OpenMind.Payment.Api.csproj", "src/Services/Payment/OpenMind.Payment.Api/"]

RUN dotnet restore "src/Services/Payment/OpenMind.Payment.Api/OpenMind.Payment.Api.csproj"

COPY . .
WORKDIR "/src/src/Services/Payment/OpenMind.Payment.Api"
RUN dotnet build "OpenMind.Payment.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OpenMind.Payment.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OpenMind.Payment.Api.dll"]
