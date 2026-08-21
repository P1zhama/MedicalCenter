FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
RUN apt-get update \
    && apt-get install -y --no-install-recommends fonts-dejavu-core libfontconfig1 \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 8080


FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore "Documents/Documents.Api/Documents.Api.csproj"

WORKDIR "/src/Documents/Documents.Api"
RUN dotnet build "Documents.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Documents.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Documents.Api.dll"]
