# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY *.csproj .
RUN dotnet restore

COPY . .
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false


# Runtime stage - use Playwright image as base
FROM mcr.microsoft.com/playwright:v1.57.0-noble AS final
WORKDIR /app

# ADD: install Xvfb
RUN apt-get update && \
    apt-get install -y xvfb wget && \
    rm -rf /var/lib/apt/lists/*

# Install .NET 10 Runtime (unchanged)
RUN wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh && \
    chmod +x dotnet-install.sh && \
    ./dotnet-install.sh --channel 10.0 --runtime aspnetcore --install-dir /usr/share/dotnet && \
    ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet && \
    rm dotnet-install.sh

# Copy published app
COPY --from=publish /app/publish .

# Expose port
EXPOSE 3000

# ADD: virtual display
ENV DISPLAY=:99
ENV ASPNETCORE_URLS=http://+:3000

# CHANGE: start Xvfb before app
ENTRYPOINT ["bash", "-c", "Xvfb :99 -screen 0 1920x1080x24 & dotnet StoreScrapper.dll"]