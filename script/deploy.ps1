$ErrorActionPreference = "Stop" # exit on error

# set version
$file = 'dist\manifest.yml'
(Get-Content $file).replace('1.0.0-dev', "$env:yak_package_version") | Set-Content $file

# copy .gha to dist/
Copy-Item -Path bin\GhCanvasViewport.gha -Destination dist\

# zip (create package manually)
Compress-Archive -Path dist\* -DestinationPath dist\build.zip

# rename file (unnecessary, but tidier)
# $filename = "build-$env:appveyor_build_number.yak"
# Rename-Item -Path dist\build.zip -NewName $filename

# publish
.\tools\yak version
.\tools\yak.exe push dist\build.zip
