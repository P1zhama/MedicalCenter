FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080


FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore "Appointments/Appointments.Api/Appointments.Api.csproj"

WORKDIR "/src/Appointments/Appointments.Api"
RUN dotnet build "Appointments.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Appointments.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Appointments.Api.dll"]
