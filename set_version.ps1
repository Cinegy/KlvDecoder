# PowerShell Cinegy Build Script
# COPYRIGHT Cinegy 2020-2022
param([string]$BuildCounter=0,[string]$SourceRevisionValue="FFFFFF",[string]$OverrideMinorVersion="")

$majorVer = 2
$minorVer = 0

#minor version may be overridden (e.g. on integration builds)
if($OverrideMinorVersion)
{
    $minorVer = $OverrideMinorVersion
}

#calculte a UInt16 from the commit hash to use as 4th version flag
$shortRev = $SourceRevisionValue.Substring(0,4)
$sourceAsDecimal = [System.Convert]::ToUInt16($shortRev, 16) -1

$softwareVersion = "$majorVer.$minorVer.$BuildCounter.$sourceAsDecimal"

#set global variable to version number
$env:SoftwareVersion = $softwareVersion

Get-ChildItem -Path *.csproj -Recurse | ForEach-Object {
    $fileName = $_
    Write-Host "Processing metadata changes for file: $fileName"
	
	[xml]$projectXml = Get-Content -Path $fileName

	$nodes = $projectXml.SelectNodes("/Project/PropertyGroup/Copyright")
	foreach($node in $nodes) {
		$node.'#text' = "$([char]0xA9)$((Get-Date).year) Cinegy. All rights reserved."
	}

	$nodes = $projectXml.SelectNodes("/Project/PropertyGroup/Description")
	foreach($node in $nodes) {
		$node.'#text' = "$($node.'#text')"
	}

	$nodes = $projectXml.SelectNodes("/Project/PropertyGroup/Version")
	foreach($node in $nodes) {
		$node.'#text' = $SoftwareVersion
	}

	$nodes = $projectXml.SelectNodes("/Project/PropertyGroup/AssemblyVersion")
	foreach($node in $nodes) {
		$node.'#text' = $SoftwareVersion
	}

	$nodes = $projectXml.SelectNodes("/Project/PropertyGroup/FileVersion")
	foreach($node in $nodes) {
		$node.'#text' = $SoftwareVersion
	}

	$projectXml.Save($fileName)

}
