FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore "Profiles/Profiles.Api/Profiles.Api.csproj"

WORKDIR "/src/Profiles/Profiles.Api"
RUN dotnet build "Profiles.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Profiles.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Profiles.Api.dll"]
