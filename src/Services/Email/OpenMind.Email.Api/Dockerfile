FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["src/BuildingBlocks/OpenMind.Shared.IntegrationMessages/OpenMind.Shared.IntegrationMessages.csproj", "src/BuildingBlocks/OpenMind.Shared.IntegrationMessages/"]
COPY ["src/Services/Email/OpenMind.Email.IntegrationMessages/OpenMind.Email.IntegrationMessages.csproj", "src/Services/Email/OpenMind.Email.IntegrationMessages/"]
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
