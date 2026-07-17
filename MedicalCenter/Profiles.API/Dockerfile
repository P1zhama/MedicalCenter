FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Profiles.API/Profiles.API.csproj", "Profiles.API/"]
COPY ["Profiles.Application/Profiles.Application.csproj", "Profiles.Application/"]
COPY ["Profiles.Domain/Profiles.Domain.csproj", "Profiles.Domain/"]
COPY ["Profiles.Infrastructure/Profiles.Infrastructure.csproj", "Profiles.Infrastructure/"]
COPY ["MedicalCenter.Shared.Contracts/MedicalCenter.Shared.Contracts.csproj", "MedicalCenter.Shared.Contracts/"]

RUN dotnet restore "Profiles.API/Profiles.API.csproj"

COPY . .
WORKDIR "/src/Profiles.API"
RUN dotnet build "Profiles.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Profiles.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Profiles.API.dll"]
