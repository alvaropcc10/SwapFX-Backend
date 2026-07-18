FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["SwapFX.API/SwapFX.API.csproj", "SwapFX.API/"]
COPY ["SwapFX.CORE/SwapFX.CORE.csproj", "SwapFX.CORE/"]

RUN dotnet restore "SwapFX.API/SwapFX.API.csproj"

COPY . .

RUN dotnet publish "SwapFX.API/SwapFX.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:5248
ENV DOTNET_USE_POLLING_FILE_WATCHER=1
ENV ASPNETCORE_hostBuilder__reloadConfigOnChange=false

COPY --from=build /app/publish .

EXPOSE 5248

ENTRYPOINT ["dotnet", "SwapFX.API.dll"]
