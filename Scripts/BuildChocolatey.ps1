param($connectionString = "")

$scriptPath = Split-Path $MyInvocation.MyCommand.Path

# Based on http://stackoverflow.com/a/26443520/18475
$MSBuildPath = "C:\Program Files (x86)\MSBuild\12.0\bin\msbuild.exe"
if (!(Test-Path($MSBuildPath))) {
  $MSBuildPath = "$env:systemRoot\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
}

$nugetProjFile = "$($scriptPath)\ChocolateyGallery.msbuild"
write-host "Running $nugetProjFile with $connectionString" 
Write-host "================================================"
Write-host "Build"
Write-host "================================================"
& $MSBuildPath "$nugetProjFile" /t:Build #/p:VisualStudioVersion=12.0 
Write-host "================================================"
Write-host "CleanBuildOutput"
Write-host "================================================"
& $MSBuildPath "$nugetProjFile" /t:CleanBuildOutput #/p:VisualStudioVersion=12.0 

# Write-host "================================================"
# Write-host "Copying chocolatey items over the nuget defaults"
# Write-host "================================================"
# $chocWeb = "$($scriptPath)\..\Website"
# $buildDest = "$($scriptPath)\..\..\bin\_PublishedWebsites"
# Write-host "Copying the contents of `'$chocWeb`' to `'$buildDest`'"
# Copy-Item $chocWeb $buildDest -recurse -force

