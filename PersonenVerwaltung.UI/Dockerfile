FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PersonenVerwaltung.UI/PersonenVerwaltung.UI.csproj", "PersonenVerwaltung.UI/"]
RUN dotnet restore "PersonenVerwaltung.UI/PersonenVerwaltung.UI.csproj"
COPY . .
WORKDIR "/src/PersonenVerwaltung.UI"
RUN dotnet build "PersonenVerwaltung.UI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PersonenVerwaltung.UI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8081
ENTRYPOINT ["dotnet", "PersonenVerwaltung.UI.dll"]
