param(
    [Parameter(Mandatory = $true)]
    [string]$UnityEditorPath
)
$ErrorActionPreference = 'Stop'
$projectRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
if (-not (Test-Path -LiteralPath (Join-Path $projectRoot 'Assets/_Game') -PathType Container)) {
    throw 'Run this tool from the AFFIX Unity project.'
}
$editor = Get-Item -LiteralPath $UnityEditorPath -ErrorAction Stop
if ($editor.Name -ne 'Unity.exe' -or -not $editor.VersionInfo.ProductVersion.StartsWith('6000.3.24f1')) {
    throw 'The approved Unity 6000.3.24f1 executable is required.'
}
$managed = Join-Path $editor.DirectoryName 'Data/Managed'
if (-not (Test-Path -LiteralPath (Join-Path $managed 'UnityEngine/UnityEngine.CoreModule.dll'))) {
    throw 'Installed Unity engine assemblies were not found.'
}
Get-Command dotnet -ErrorAction Stop | Out-Null
$outputDirectory = Join-Path $projectRoot 'Build/Validation'
New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
$engineReference = [System.Security.SecurityElement]::Escape(($managed + '/UnityEngine/*.dll').Replace('\', '/'))
$editorReference = [System.Security.SecurityElement]::Escape(($managed + '/UnityEditor.*.dll').Replace('\', '/'))
$project = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
    <GenerateAssemblyInfo>false</GenerateAssemblyInfo>
    <DefineConstants>UNITY_EDITOR;UNITY_STANDALONE_WIN;UNITY_6000_3</DefineConstants>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="../../Assets/_Game/**/*.cs" />
    <Reference Include="$engineReference" />
    <Reference Include="$editorReference" />
  </ItemGroup>
</Project>
"@
$projectPath = Join-Path $outputDirectory 'UnityStaticValidation.csproj'
[System.IO.File]::WriteAllText($projectPath, $project)
Push-Location -LiteralPath $projectRoot
try {
    & dotnet build $projectPath --configuration Release --nologo
    if ($LASTEXITCODE -ne 0) { throw 'Unity reference static compilation failed.' }
    Write-Output 'STATIC_CSHARP_COMPILE_PASSED: Installed engine API references only. NOT Unity import, asmdef/package resolution, Play, rendering or player build.'
}
finally { Pop-Location }
