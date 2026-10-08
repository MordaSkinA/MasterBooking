FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
RUN apt-get update && apt-get install -y libfontconfig1

WORKDIR /src

# Copy all project files to restore dependencies
COPY ["src/MasterBooking.Domain/MasterBooking.Domain.csproj", "src/MasterBooking.Domain/"]
COPY ["src/MasterBooking.Infrastructure/MasterBooking.Infrastructure.csproj", "src/MasterBooking.Infrastructure/"]
COPY ["src/MasterBooking.Web/MasterBooking.Web.csproj", "src/MasterBooking.Web/"]

# Restore dependencies
RUN dotnet restore "src/MasterBooking.Web/MasterBooking.Web.csproj"

# Copy the rest of the source code
COPY . .

# Build and publish the application
RUN dotnet publish "src/MasterBooking.Web/MasterBooking.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage: Use the ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
RUN apt-get update && apt-get install -y libfontconfig1 && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app/publish .

# Create uploads folder
RUN mkdir -p uploads

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "MasterBooking.Web.dll"]
