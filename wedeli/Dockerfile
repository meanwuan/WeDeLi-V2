# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["wedeli.csproj", "./"]
RUN dotnet restore "wedeli.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "wedeli.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "wedeli.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

# Copy published app
COPY --from=publish /app/publish .

# Set environment to Production
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "wedeli.dll"]
