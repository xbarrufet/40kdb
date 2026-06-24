FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY backend/40kdb/*.csproj ./
RUN dotnet restore

COPY backend/40kdb/ ./
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .
COPY data/ /seed-data/

ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 5050
ENV ASPNETCORE_URLS=http://+:5050

ENTRYPOINT ["dotnet", "40kdb.dll"]
