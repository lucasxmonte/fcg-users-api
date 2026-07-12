FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Feed local com FCG.Contracts
COPY packages/ packages/
COPY NuGet.Config .

COPY ["src/FCG.UsersAPI.API/FCG.UsersAPI.API.csproj",                      "src/FCG.UsersAPI.API/"]
COPY ["src/FCG.UsersAPI.Application/FCG.UsersAPI.Application.csproj",      "src/FCG.UsersAPI.Application/"]
COPY ["src/FCG.UsersAPI.Domain/FCG.UsersAPI.Domain.csproj",                "src/FCG.UsersAPI.Domain/"]
COPY ["src/FCG.UsersAPI.Infrastructure/FCG.UsersAPI.Infrastructure.csproj","src/FCG.UsersAPI.Infrastructure/"]

RUN dotnet restore "src/FCG.UsersAPI.API/FCG.UsersAPI.API.csproj" \
    --configfile ./NuGet.Config

COPY . .

WORKDIR "/src/src/FCG.UsersAPI.API"
RUN dotnet publish "FCG.UsersAPI.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Docker
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FCG.UsersAPI.API.dll"]
