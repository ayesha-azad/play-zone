# -----------------
# Build Stage
# -----------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# copy csproj files first for caching
COPY PlayZone.BLL/PlayZone.BLL.csproj PlayZone.BLL/
COPY PlayZone.DAL/PlayZone.DAL.csproj PlayZone.DAL/
COPY PlayZone.PL/PlayZone.PL.csproj PlayZone.PL/

# restore dependencies
RUN dotnet restore PlayZone.PL/PlayZone.PL.csproj

# copy everything
COPY . .

# build & publish
RUN dotnet publish PlayZone.PL/PlayZone.PL.csproj -c Release -o /publish

# -----------------
# Runtime Stage
# -----------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# copy published files
COPY --from=build /publish .

EXPOSE 8080

# run the compiled DLL
ENTRYPOINT ["dotnet", "PlayZone.PL.dll"]