FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

EXPOSE 8080


FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore "Offices/Offices.Api/Offices.Api.csproj"

WORKDIR "/src/Offices/Offices.Api"
RUN dotnet build "Offices.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Offices.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Offices.Api.dll"]
