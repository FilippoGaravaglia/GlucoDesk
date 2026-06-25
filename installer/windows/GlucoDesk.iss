#define MyAppName "GlucoDesk"

#ifndef MyAppVersion
#define MyAppVersion "0.2.1-preview"
#endif

#ifndef RuntimeIdentifier
#define RuntimeIdentifier "win-x64"
#endif

#ifndef SourceDir
#define SourceDir "..\..\artifacts\windows\0.2.1-preview\win-x64\publish"
#endif

#ifndef OutputDir
#define OutputDir "..\..\artifacts\windows\0.2.1-preview\win-x64\installer"
#endif

#define MyAppPublisher "Filippo Garavaglia"
#define MyAppURL "https://github.com/FilippoGaravaglia/GlucoDesk"
#define MyAppExeName "GlucoDesk.Desktop.exe"
#define MyAppId "{{6E0D40F7-9E18-43A2-9F13-4A7E53C9C6B7}"

[Setup]
AppId={#MyAppId}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}

DefaultDirName={localappdata}\Programs\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes

OutputDir={#OutputDir}
OutputBaseFilename={#MyAppName}-{#MyAppVersion}-{#RuntimeIdentifier}-setup

Compression=lzma2
SolidCompression=yes
WizardStyle=modern
SetupLogging=yes

PrivilegesRequired=lowest
MinVersion=10.0

ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

UninstallDisplayName={#MyAppName}
UninstallDisplayIcon={app}\{#MyAppExeName}

CloseApplications=yes
RestartApplications=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"
Name: "{group}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}"
