#define AppVersion "0.2.3"
[Setup]
AppId={{C5523D33-90BE-4AAB-9508-E0770C6BA501}
AppName=Segata Sakura
AppVersion={#AppVersion}
AppPublisher=Segata Sakura Project
DefaultDirName={code:DefaultInstallDir}
DefaultGroupName=Segata Sakura
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
MinVersion=10.0
OutputDir=..\installateur
OutputBaseFilename=SegataSakura-Setup-{#AppVersion}-x64
SetupIconFile=sakura-v022.ico
UninstallDisplayIcon={app}\sakura-v022.ico
Compression=lzma2/fast
SolidCompression=yes
WizardStyle=modern
WizardSizePercent=110
CloseApplications=yes
RestartApplications=no
DisableWelcomePage=no
DisableDirPage=no
DisableReadyPage=no
ShowLanguageDialog=no
InfoBeforeFile=INSTALLATION.txt
UninstallDisplayName=Segata Sakura
CreateUninstallRegKey=not IsSmokeTest

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Shortcuts:"

[Files]
Source: "SegataSakura-0.2.3.exe"; DestDir: "{app}"; DestName: "SegataSakura.exe"; Flags: ignoreversion
Source: "sakura-logo.png"; DestDir: "{app}"; Flags: ignoreversion
Source: "sakura-v022.ico"; DestDir: "{app}"; Flags: ignoreversion
Source: "GUIDE.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: "NOTICES.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: "KRONOS-COPYING.txt"; DestDir: "{app}\licenses"; Flags: ignoreversion
Source: "SDL-LICENSE.txt"; DestDir: "{app}\licenses"; Flags: ignoreversion
Source: "licenses\*"; DestDir: "{app}\licenses"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "engine\*"; DestDir: "{app}\engine"; Excludes: "*.ini,*.bak,*.log,*.exe,*.bin,*.ram,*.yss"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "engine\kronos.exe"; DestDir: "{app}\engine"; Flags: ignoreversion
Source: "engine-default.ini"; DestDir: "{app}\engine"; DestName: "kronos.ini"; Flags: onlyifdoesntexist uninsneveruninstall
Source: "engine\vc_redist.x64.exe"; DestDir: "{app}\prerequisites"; Flags: ignoreversion
Source: "sources\*"; DestDir: "{app}\sources"; Flags: ignoreversion
Source: "src\*.cs"; DestDir: "{app}\sources\segata-sakura\src"; Flags: ignoreversion
Source: "build.ps1"; DestDir: "{app}\sources\segata-sakura"; Flags: ignoreversion
Source: "app.manifest"; DestDir: "{app}\sources\segata-sakura"; Flags: ignoreversion
Source: "sakura-v022.ico"; DestDir: "{app}\sources\segata-sakura"; Flags: ignoreversion
Source: "installer.iss"; DestDir: "{app}\sources\segata-sakura"; Flags: ignoreversion

[Icons]
Name: "{userprograms}\Segata Sakura"; Filename: "{app}\SegataSakura.exe"; WorkingDir: "{app}"; IconFilename: "{app}\sakura-v022.ico"; IconIndex: 0; Check: not IsSmokeTest
Name: "{userdesktop}\Segata Sakura"; Filename: "{app}\SegataSakura.exe"; WorkingDir: "{app}"; Tasks: desktopicon; Check: not IsSmokeTest

[Run]
Filename: "{app}\SegataSakura.exe"; Description: "Launch Segata Sakura"; Flags: nowait postinstall skipifsilent

[Code]
function IsSmokeTest: Boolean;
begin
 Result := ExpandConstant('{param:SMOKETEST|0}') = '1';
end;

function DefaultInstallDir(Param: String): String;
begin
 Result := GetEnv('LOCALAPPDATA') + '\Programs\SegataSakura';
end;

function NeedsVCRuntime: Boolean;
var Installed, Build: Cardinal;
begin
 Result := not (RegQueryDWordValue(HKLM64, 'SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64', 'Installed', Installed) and (Installed = 1) and
 RegQueryDWordValue(HKLM64, 'SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64', 'Bld', Build) and (Build >= 29016));
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
var Code: Integer;
begin
 Result := '';
 if IsSmokeTest then exit;
 if NeedsVCRuntime then begin
  ExtractTemporaryFile('vc_redist.x64.exe');
  if not ShellExec('runas', ExpandConstant('{tmp}\vc_redist.x64.exe'), '/install /passive /norestart', '', SW_SHOWNORMAL, ewWaitUntilTerminated, Code) then
   Result := 'Kronos requires Microsoft Visual C++. Installation was cancelled or denied. Run this installer again to retry.'
  else if (Code <> 0) and (Code <> 3010) and (Code <> 1638) then
   Result := 'Microsoft Visual C++ installation failed (code ' + IntToStr(Code) + ').'
  else if Code = 3010 then NeedsRestart := True;
 end;
end;
