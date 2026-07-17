FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

EXPOSE 8080


FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Authorization.API/Authorization.API.csproj", "Authorization.API/"]
COPY ["Authorization.Application/Authorization.Application.csproj", "Authorization.Application/"]
COPY ["Authorization.Domain/Authorization.Domain.csproj", "Authorization.Domain/"]
COPY ["Authorization.Infrastructure/Authorization.Infrastructure.csproj", "Authorization.Infrastructure/"]
COPY ["MedicalCenter.Shared.Contracts/MedicalCenter.Shared.Contracts.csproj", "MedicalCenter.Shared.Contracts/"]

RUN dotnet restore "Authorization.API/Authorization.API.csproj"

COPY . .
WORKDIR "/src/Authorization.API"
RUN dotnet build "Authorization.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Authorization.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Authorization.API.dll"]
