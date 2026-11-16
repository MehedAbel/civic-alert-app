#!/bin/bash
set -e

# If dotnet is not in PATH, check /usr/share/dotnet or install it
if ! command -v dotnet &> /dev/null; then
    if [ -f /usr/share/dotnet/dotnet ]; then
        export PATH="/usr/share/dotnet:$PATH"
    elif [ -f /tmp/dotnet/dotnet ]; then
        export PATH="/tmp/dotnet:$PATH"
    else
        echo "Installing .NET SDK..."
        curl -sSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
        bash /tmp/dotnet-install.sh --channel 10.0 --install-dir /usr/share/dotnet
        export PATH="/usr/share/dotnet:$PATH"
        ln -sf /usr/share/dotnet/dotnet /usr/bin/dotnet || true
    fi
fi

export ASPNETCORE_URLS="http://0.0.0.0:3000"
export DOTNET_CLI_TELEMETRY_OPTOUT=1

cd /app/applet/CivicAlert
exec dotnet run --urls "http://0.0.0.0:3000"
