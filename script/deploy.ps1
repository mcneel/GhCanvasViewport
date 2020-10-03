# script/deploy.ps1: Create a yak package and push it to the server

$ErrorActionPreference = "Stop" # exit on error

Push-Location
cd (Split-Path $MyInvocation.MyCommand.Path)\..\dist
# $dist = '..\dist'

# get yak.exe
$url = 'http://files.mcneel.com/yak/tools/latest/yak.exe'
$yak = '..\yak.exe'
# Write-Host (New-Object System.Net.WebClient).DownloadFile($url, $yak) # not working
Invoke-WebRequest -Uri $url -OutFile $yak
& $yak version

# copy .gha to dist/
# TODO: make script generic by moving files to command line args?
Copy-Item -Path ..\bin\GhCanvasViewport.gha -Destination .

# build package
& $yak build #--version $env:yak_package_version

# publish
& $yak push *.yak

Pop-Location
