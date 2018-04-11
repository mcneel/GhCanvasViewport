$ErrorActionPreference = "Stop" # exit on error

# set version
$file = 'dist\manifest.yml'
$build = $env:appveyor_build_number
(Get-Content $file).replace('1.0.0-dev', "1.0.0-dev.$build") | Set-Content $file

# copy .gha to dist/
Copy-Item -Path bin\GhCanvasViewport.gha -Destination dist\

# zip (create package manually)
Compress-Archive -Path dist\* -DestinationPath dist\build.zip

$filename = "build-$build.yak"
Rename-Item -Path dist\build.zip -NewName $filename

# publish
.\tools\yak.exe push dist\$filename
