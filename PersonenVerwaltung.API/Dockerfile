FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PersonenVerwaltung.API/PersonenVerwaltung.API.csproj", "PersonenVerwaltung.API/"]
COPY ["PersonenVerwaltung.Data/PersonenVerwaltung.Data.csproj", "PersonenVerwaltung.Data/"]
RUN dotnet restore "PersonenVerwaltung.API/PersonenVerwaltung.API.csproj"
COPY . .
WORKDIR "/src/PersonenVerwaltung.API"
RUN dotnet build "PersonenVerwaltung.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PersonenVerwaltung.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "PersonenVerwaltung.API.dll"]
