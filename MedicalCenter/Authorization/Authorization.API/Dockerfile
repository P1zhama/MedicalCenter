FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

EXPOSE 8080


FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Authorization/Authorization.Api/Authorization.Api.csproj", "Authorization/Authorization.Api/"]
COPY ["Authorization/Authorization.Application/Authorization.Application.csproj", "Authorization/Authorization.Application/"]
COPY ["Authorization/Authorization.Domain/Authorization.Domain.csproj", "Authorization/Authorization.Domain/"]
COPY ["Authorization/Authorization.Infrastructure/Authorization.Infrastructure.csproj", "Authorization/Authorization.Infrastructure/"]
COPY ["MedicalCenter.Shared.Contracts/MedicalCenter.Shared.Contracts.csproj", "MedicalCenter.Shared.Contracts/"]

RUN dotnet restore "Authorization/Authorization.Api/Authorization.Api.csproj"

COPY . .
WORKDIR "/src/Authorization/Authorization.Api"
RUN dotnet build "Authorization.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Authorization.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Authorization.Api.dll"]
