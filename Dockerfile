# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY HotelReservation.Core/*.csproj HotelReservation.Core/
COPY HotelReservation.Data/*.csproj HotelReservation.Data/
COPY HotelReservation.Services/*.csproj HotelReservation.Services/
COPY HotelReservation.Web/*.csproj HotelReservation.Web/

# Restore dependencies
RUN dotnet restore HotelReservation.Web/HotelReservation.Web.csproj

# Copy everything else
COPY . .

# Build and publish
RUN dotnet publish HotelReservation.Web/HotelReservation.Web.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Railway uses PORT environment variable
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}

EXPOSE 8080
ENTRYPOINT ["dotnet", "HotelReservation.Web.dll"]
