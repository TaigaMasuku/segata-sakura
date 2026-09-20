param([string]$OutputName = 'SegataSakura-0.2.exe')
$ErrorActionPreference = 'Stop'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
Push-Location $PSScriptRoot
try {
    & $compiler /nologo /target:winexe /optimize+ "/out:$OutputName" /win32icon:sakura.ico /win32manifest:app.manifest /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Xml.Linq.dll src\Saturn.cs src\FolderPicker.cs src\AppPaths.cs src\AssemblyInfo.cs
    if ($LASTEXITCODE -ne 0) { throw 'Compilation impossible.' }
} finally { Pop-Location }
