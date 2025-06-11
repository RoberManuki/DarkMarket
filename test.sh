#!/bin/bash
dotnet clean
rm -rf bin obj DarkMarket.Tests/bin DarkMarket.Tests/obj
dotnet restore
dotnet test DarkMarket.Tests -v n