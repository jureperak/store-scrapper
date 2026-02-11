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

# Install Xvfb + wget (unchanged)
RUN apt-get update && \
    apt-get install -y xvfb wget && \
    rm -rf /var/lib/apt/lists/*

# Install .NET 10 Runtime (unchanged)
RUN wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh && \
    chmod +x dotnet-install.sh && \
    ./dotnet-install.sh --channel 10.0 --runtime aspnetcore --install-dir /usr/share/dotnet && \
    ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet && \
    rm dotnet-install.sh

COPY --from=publish /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# ✅ Run Playwright headed reliably using xvfb-run (auto-picks a free DISPLAY)
ENTRYPOINT ["xvfb-run", "-a", "-s", "-screen 0 1920x1080x24", "dotnet", "StoreScrapper.dll"]
