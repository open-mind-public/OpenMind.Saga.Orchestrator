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
COPY ["src/Services/Email/OpenMind.Email.Domain/OpenMind.Email.Domain.csproj", "src/Services/Email/OpenMind.Email.Domain/"]
COPY ["src/Services/Email/OpenMind.Email.Application/OpenMind.Email.Application.csproj", "src/Services/Email/OpenMind.Email.Application/"]
COPY ["src/Services/Email/OpenMind.Email.Infrastructure/OpenMind.Email.Infrastructure.csproj", "src/Services/Email/OpenMind.Email.Infrastructure/"]
COPY ["src/Services/Email/OpenMind.Email.Api/OpenMind.Email.Api.csproj", "src/Services/Email/OpenMind.Email.Api/"]

RUN dotnet restore "src/Services/Email/OpenMind.Email.Api/OpenMind.Email.Api.csproj"

COPY . .
WORKDIR "/src/src/Services/Email/OpenMind.Email.Api"
RUN dotnet build "OpenMind.Email.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OpenMind.Email.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OpenMind.Email.Api.dll"]
