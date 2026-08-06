FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Profiles/Profiles.Api/Profiles.Api.csproj", "Profiles/Profiles.Api/"]
COPY ["Profiles/Profiles.Application/Profiles.Application.csproj", "Profiles/Profiles.Application/"]
COPY ["Profiles/Profiles.Domain/Profiles.Domain.csproj", "Profiles/Profiles.Domain/"]
COPY ["Profiles/Profiles.Infrastructure/Profiles.Infrastructure.csproj", "Profiles/Profiles.Infrastructure/"]
COPY ["MedicalCenter.Shared.Contracts/MedicalCenter.Shared.Contracts.csproj", "MedicalCenter.Shared.Contracts/"]

RUN dotnet restore "Profiles/Profiles.Api/Profiles.Api.csproj"

COPY . .
WORKDIR "/src/Profiles/Profiles.Api"
RUN dotnet build "Profiles.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Profiles.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Profiles.Api.dll"]
