#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Corvus.Deployment.PowerBi.Cli/Corvus.Deployment.PowerBi.Cli.csproj", "Corvus.Deployment.PowerBi.Cli/"]
RUN dotnet restore "Corvus.Deployment.PowerBi.Cli/Corvus.Deployment.PowerBi.Cli.csproj"
COPY . .
WORKDIR "/src/Corvus.Deployment.PowerBi.Cli"
RUN dotnet build "Corvus.Deployment.PowerBi.Cli.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Corvus.Deployment.PowerBi.Cli.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Corvus.Deployment.PowerBi.Cli.dll"]