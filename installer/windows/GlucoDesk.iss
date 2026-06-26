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

#ifndef LicenseFilePath
#define LicenseFilePath "..\..\LICENSE"
#endif

#ifndef InfoBeforeFilePath
#define InfoBeforeFilePath "WINDOWS-INSTALLER-SAFETY-NOTICE.txt"
#endif

#ifndef InfoAfterFilePath
#define InfoAfterFilePath "WINDOWS-INSTALLER-AFTER-INSTALL.txt"
#endif

#define MyAppPublisher "Filippo Garavaglia"
#define MyAppURL "https://github.com/FilippoGaravaglia/GlucoDesk"
#define MyAppExeName "GlucoDesk.Desktop.exe"
#define MyAppId "{{6E0D40F7-9E18-43A2-9F13-4A7E53C9C6B7}"

[Setup]
AppId={#MyAppId}
AppName={#MyAppName}
AppVerName={#MyAppName} {#MyAppVersion}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
AppComments=A local-first desktop companion for glucose awareness.

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

VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription={#MyAppName} Windows Preview Installer
VersionInfoProductName={#MyAppName}

LicenseFile={#LicenseFilePath}
InfoBeforeFile={#InfoBeforeFilePath}
InfoAfterFile={#InfoAfterFilePath}

CloseApplications=yes
RestartApplications=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"
Name: "{group}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; Check: ShouldCreateDesktopIcon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}"

[Code]

var
  AdditionalTasksPage: TWizardPage;
  DesktopIconCheckBox: TNewCheckBox;

procedure InitializeWizard;
var
  DescriptionLabel: TNewStaticText;
  GroupLabel: TNewStaticText;
begin
  AdditionalTasksPage := CreateCustomPage(
    wpSelectDir,
    'Select Additional Tasks',
    'Which additional tasks should be performed?'
  );

  DescriptionLabel := TNewStaticText.Create(WizardForm);
  DescriptionLabel.Parent := AdditionalTasksPage.Surface;
  DescriptionLabel.Left := ScaleX(8);
  DescriptionLabel.Top := ScaleY(8);
  DescriptionLabel.Width := AdditionalTasksPage.SurfaceWidth - ScaleX(16);
  DescriptionLabel.Height := ScaleY(32);
  DescriptionLabel.Caption := 'Choose whether GlucoDesk should create additional shortcuts.';

  GroupLabel := TNewStaticText.Create(WizardForm);
  GroupLabel.Parent := AdditionalTasksPage.Surface;
  GroupLabel.Left := ScaleX(32);
  GroupLabel.Top := ScaleY(76);
  GroupLabel.Width := AdditionalTasksPage.SurfaceWidth - ScaleX(40);
  GroupLabel.Height := ScaleY(20);
  GroupLabel.Caption := 'Additional shortcuts:';

  DesktopIconCheckBox := TNewCheckBox.Create(WizardForm);
  DesktopIconCheckBox.Parent := AdditionalTasksPage.Surface;
  DesktopIconCheckBox.Left := ScaleX(32);
  DesktopIconCheckBox.Top := ScaleY(108);
  DesktopIconCheckBox.Width := AdditionalTasksPage.SurfaceWidth - ScaleX(40);
  DesktopIconCheckBox.Height := ScaleY(24);
  DesktopIconCheckBox.Caption := 'Create a desktop shortcut';
  DesktopIconCheckBox.Checked := False;
end;

function ShouldCreateDesktopIcon: Boolean;
begin
  Result := False;

  if DesktopIconCheckBox <> nil then
    Result := DesktopIconCheckBox.Checked;
end;
