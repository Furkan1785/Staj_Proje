FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SakaryaERP.csproj", "."]
RUN dotnet restore "./SakaryaERP.csproj"
COPY . .
RUN dotnet build "SakaryaERP.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SakaryaERP.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SakaryaERP.dll"]
