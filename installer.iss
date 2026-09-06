; DeepSeek Desktop installer
#define MyAppName "DeepSeek Desktop"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "DeepSeek"
#define MyAppExeName "DeepSeekDesktop.exe"
#define MyAppUrl "https://chat.deepseek.com/"

[Setup]
AppId={{B7A4F2C8-2D3E-4C9A-8F6B-DeepSeekDesktop}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppUrl}
AppSupportURL={#MyAppUrl}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputDir=.
OutputBaseFilename=DeepSeekDesktop-Setup
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
SetupIconFile=Assets\deepseek.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
MinVersion=10.0.17763
LanguageDetectionMethod=none
VersionInfoVersion={#MyAppVersion}

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "dist\app\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{userappdata}\DeepSeekDesktop\EBWebView"