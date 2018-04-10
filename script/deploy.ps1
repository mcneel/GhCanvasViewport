$ErrorActionPreference = "Stop" # exit on error

# set version
$file = 'dist\manifest.yml'
(Get-Content $file) -replace "version:\s*\S+", "version: $env:yak_package_version" | Set-Content $file

# copy .gha to dist/
Copy-Item -Path bin\GhCanvasViewport.gha -Destination dist\

# zip (create package manually)
Compress-Archive -Path dist\* -DestinationPath dist\build.zip

# publish
.\tools\yak version
.\tools\yak.exe push dist\build.zip
