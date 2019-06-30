Param(
  [parameter(Mandatory=$false)][string]$repo="https://api.nuget.org/v3/index.json",
  [parameter(Mandatory=$false)][bool]$push=$false,
	[parameter(Mandatory=$false)][string]$apikey,
	[parameter(Mandatory=$false)][bool]$build=$true
)

# Paths
$packFolder = (Get-Item -Path "./" -Verbose).FullName
$slnPath = Join-Path $packFolder ".."
$srcPath = Join-Path $packFolder "../Surging.Core"



$projects = (Get-Content "./Components")

function Pack($projectFolder,$projectName) {
  Set-Location $projectFolder
  $releaseProjectFolder = (Join-Path $projectFolder "bin/Release")
  if (Test-Path $releaseProjectFolder)
  {
     Remove-Item -Force -Recurse $releaseProjectFolder
  }
  
   & dotnet msbuild /p:Configuration=Release /p:SourceLinkCreate=true
   & dotnet msbuild /t:pack /p:Configuration=Release /p:SourceLinkCreate=true
   if ($projectName) {
    $projectPackPath = Join-Path $projectFolder ("/bin/Release/" + $projectName + ".*.nupkg")
   }else {
    $projectPackPath = Join-Path $projectFolder ("/bin/Release/" + $project + ".*.nupkg")
   }
   Move-Item -Force $projectPackPath $packFolder 
}

if ($build) {
  Set-Location $slnPath
  & dotnet restore Surging.sln

  foreach($project in $projects) {
    Pack -projectFolder (Join-Path $srcPath $project)
  }
  $webSocketProjectFolder = Join-Path $slnPath "WebSocket/WebSocketCore"   
  Pack -projectFolder $webSocketProjectFolder -projectName "Surging.WebSocketCore"

  $dotnettyCodecDns = Join-Path $slnPath "DotNetty.Codecs/DotNetty.Codecs.DNS" 
  Pack -projectFolder $dotnettyCodecDns -projectName "DotNetty.Codecs.DNS"

  Set-Location $packFolder
}

if($push) {
    if ([string]::IsNullOrEmpty($apikey)){
        Write-Warning -Message "未设置nuget仓库的APIKEY"
		exit 1
	}
	dotnet nuget push *.nupkg -s $repo -k $apikey
}
