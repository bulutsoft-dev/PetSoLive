#!/bin/bash
set -e

echo "Building solution..."
dotnet build

echo "Running all tests..."
dotnet test PetSoLive.Tests/PetSoLive.Tests.csproj --logger "console;verbosity=normal"

echo "All tests completed." 