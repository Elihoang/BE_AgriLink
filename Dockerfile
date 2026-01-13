# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["AgriLink_DH.Api/AgriLink_DH.Api.csproj", "AgriLink_DH.Api/"]
COPY ["AgriLink_DH.Core/AgriLink_DH.Core.csproj", "AgriLink_DH.Core/"]
COPY ["AgriLink_DH.Domain/AgriLink_DH.Domain.csproj", "AgriLink_DH.Domain/"]
COPY ["AgriLink_DH.Share/AgriLink_DH.Share.csproj", "AgriLink_DH.Share/"]
RUN dotnet restore "AgriLink_DH.Api/AgriLink_DH.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/AgriLink_DH.Api"
RUN dotnet build "AgriLink_DH.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "AgriLink_DH.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy published app
COPY --from=publish /app/publish .

# Create non-root user
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "AgriLink_DH.Api.dll"]
