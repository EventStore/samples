FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["EventStoreDB.Dotnet.Sample.csproj", "./"]
RUN dotnet restore "EventStoreDB.Dotnet.Sample.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "EventStoreDB.Dotnet.Sample.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "EventStoreDB.Dotnet.Sample.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "EventStoreDB.Dotnet.Sample.dll"]
