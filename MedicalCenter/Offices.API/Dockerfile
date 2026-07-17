FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

EXPOSE 8080


FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Offices.API/Offices.API.csproj", "Offices.API/"]
COPY ["Offices.Application/Offices.Application.csproj", "Offices.Application/"]
COPY ["Offices.Domain/Offices.Domain.csproj", "Offices.Domain/"]
COPY ["Offices.Infrastructure/Offices.Infrastructure.csproj", "Offices.Infrastructure/"]
COPY ["MedicalCenter.Shared.Contracts/MedicalCenter.Shared.Contracts.csproj", "MedicalCenter.Shared.Contracts/"]

RUN dotnet restore "Offices.API/Offices.API.csproj"

COPY . .
WORKDIR "/src/Offices.API"
RUN dotnet build "Offices.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Offices.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Offices.API.dll"]
