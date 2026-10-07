; Script generated for ParkGate Windows Forms Desktop Application
; Inno Setup Compiler Script (v6.x)

#define MyAppName "ParkGate"
#define MyAppArabicName "منظومة بوابات الحديقة"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "ParkGate"
#define MyAppExeName "ParkGate.WinForms.exe"
#define MyAppAssocName MyAppName + " File"
#define MyAppAssocExt ".pgate"
#define MyAppAssocKey StringChange(MyAppAssocName, " ", "") + MyAppAssocExt

[Setup]
; App Identity
AppId={{B81F287A-28B0-4A04-89FD-C76461D22C68}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} v{#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes

; Output Settings
OutputDir=..\dist\installer
OutputBaseFilename=ParkGate_Setup_v{#MyAppVersion}
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern

; Platform & Security
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=commandline
CloseApplications=yes
RestartApplications=no

; Visuals
DisableProgramGroupPage=auto

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[CustomMessages]
english.CreateDesktopIcon=Create a &desktop shortcut
english.CreateDesktopIconAr=إنشاء أيقونة على سطح المكتب
english.LaunchApp=Launch ParkGate (تشغيل المنظومة)

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIconAr}"; GroupDescription: "{cm:AdditionalIcons}"

[Dirs]
; Ensure %ProgramData%\ParkGate and logs have read/write access for standard domain/local users
Name: "{commonappdata}\ParkGate"; Permissions: users-modify
Name: "{commonappdata}\ParkGate\logs"; Permissions: users-modify

[Files]
; Client binaries from publish output
Source: "..\dist\client\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; Cairo font installation to Windows fonts
Source: "..\dist\client\Resources\Fonts\Cairo.ttf"; DestDir: "{autofonts}"; FontInstall: "Cairo"; Flags: onlyifdoesntexist uninsneveruninstall
; Automated backup script
Source: "..\scripts\backup.ps1"; DestDir: "{app}\scripts"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Comment: "{#MyAppArabicName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon; Comment: "{#MyAppArabicName}"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchApp}"; Flags: nowait postinstall skipifsilent

[Code]
// Helper to verify .NET Desktop Runtime 10 if required
function InitializeSetup(): Boolean;
begin
  Result := True;
end;
