#!/usr/bin/env bash
set -e
dotnet tool restore
dotnet tool run mgcb /@:"Content/Content.mgcb" /platform:DesktopGL
