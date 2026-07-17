FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Profiles/Profiles.API/Profiles.API.csproj", "Profiles/Profiles.API/"]
COPY ["Profiles/Profiles.Application/Profiles.Application.csproj", "Profiles/Profiles.Application/"]
COPY ["Profiles/Profiles.Domain/Profiles.Domain.csproj", "Profiles/Profiles.Domain/"]
COPY ["Profiles/Profiles.Infrastructure/Profiles.Infrastructure.csproj", "Profiles/Profiles.Infrastructure/"]
COPY ["MedicalCenter.Shared.Contracts/MedicalCenter.Shared.Contracts.csproj", "MedicalCenter.Shared.Contracts/"]

RUN dotnet restore "Profiles/Profiles.API/Profiles.API.csproj"

COPY . .
WORKDIR "/src/Profiles/Profiles.API"
RUN dotnet build "Profiles.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Profiles.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Profiles.API.dll"]
