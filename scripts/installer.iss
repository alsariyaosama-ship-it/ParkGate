; Script generated for ParkGate Windows Forms Desktop Application
; Inno Setup Compiler Script (v6.x)

#define MyAppName "ParkGate"
#define MyAppArabicName "بوابة منتزه المرح العائلي"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "منتزه المرح العائلي"
#define MyAppExeName "ParkGate.WinForms.exe"
#define MyAppAssocName MyAppName + " File"
#define MyAppAssocExt ".pgate"
#define MyAppAssocKey StringChange(MyAppAssocName, " ", "") + MyAppAssocExt

[Setup]
; App Identity
AppId={{B81F287A-28B0-4A04-89FD-C76461D22C68}
AppName={#MyAppArabicName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppArabicName} v{#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppArabicName}
AllowNoIcons=yes
SetupIconFile=..\src\ParkGate.WinForms\Resources\Icons\app.ico
UninstallDisplayIcon={app}\{#MyAppExeName}

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
; Setup production database script (for server mode)
Source: "..\scripts\Setup-ProductionDatabase.ps1"; DestDir: "{app}\scripts"; Flags: ignoreversion; Check: IsServerMode
; Database migration bundle (installed and executed in server mode only)
Source: "..\dist\database\ParkGate.Migrations.exe"; DestDir: "{app}\database"; Flags: ignoreversion; Check: IsServerMode

[Icons]
Name: "{group}\{#MyAppArabicName}"; Filename: "{app}\{#MyAppExeName}"; Comment: "{#MyAppArabicName}"; IconFilename: "{app}\Resources\Icons\app.ico"
Name: "{group}\{cm:UninstallProgram,{#MyAppArabicName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppArabicName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon; Comment: "{#MyAppArabicName}"; IconFilename: "{app}\Resources\Icons\app.ico"

[Run]
; 1. Configure server (run migrations, save connection, set firewall) in Server mode only
Filename: "powershell.exe"; Parameters: "-NoProfile -ExecutionPolicy Bypass -File ""{app}\scripts\Setup-ProductionDatabase.ps1"" -SqlServer "".\SQLEXPRESS"" -Database ""ParkGate"" -MigrationBundlePath ""{app}\database\ParkGate.Migrations.exe"""; StatusMsg: "جاري إنشاء وترقية قاعدة البيانات وضبط الاتصال بالخادم..."; Flags: runhidden; Check: IsServerMode

; 2. Launch ParkGate application
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchApp}"; Flags: nowait postinstall skipifsilent

[Code]
var
  InstallTypePage: TInputOptionWizardPage;

procedure InitializeWizard;
var
  HasSqlServer: Boolean;
begin
  InstallTypePage := CreateInputOptionPage(
    wpSelectDir,
    'نوع التثبيت (Installation Mode)',
    'تحديد دور هذا الجهاز في المنظومة (خادم رئيسي أم جهاز شباك)',
    'يرجى تحديد نمط التثبيت المناسب لهذا الجهاز:' + #13#10 +
    '- السيرفر: يقوم بتهيئة وترقية قاعدة البيانات وضبط الاتصال تلقائياً.' + #13#10 +
    '- الشباك: تثبيت واجهة المنظومة للربط مع السيرفر عبر الشبكة بدون ميقريشن.',
    True, False);

  InstallTypePage.Add('تثبيت خادم رئيسي (Server) - يشمل إنشاء وترقية قاعدة البيانات وضبط الاتصال المحلي');
  InstallTypePage.Add('تثبيت جهاز شباك / بوابة (Client) - تثبيت الواجهة فقط للربط مع السيرفر عبر الشبكة');

  // الاكتشاف التلقائي الذكي لخادم SQL Server
  HasSqlServer := RegKeyExists(HKLM, 'SOFTWARE\Microsoft\Microsoft SQL Server') or
                  RegKeyExists(HKLM, 'SOFTWARE\WOW6432Node\Microsoft\Microsoft SQL Server') or
                  RegKeyExists(HKLM, 'SYSTEM\CurrentControlSet\Services\MSSQL$SQLEXPRESS') or
                  RegKeyExists(HKLM, 'SYSTEM\CurrentControlSet\Services\MSSQLSERVER');

  if HasSqlServer then
    InstallTypePage.SelectedValueIndex := 0
  else
    InstallTypePage.SelectedValueIndex := 1;
end;

function IsServerMode: Boolean;
var
  CmdMode: String;
begin
  CmdMode := ExpandConstant('{param:MODE|none}');
  if CompareText(CmdMode, 'server') = 0 then
    Result := True
  else if CompareText(CmdMode, 'client') = 0 then
    Result := False
  else
    Result := (InstallTypePage.SelectedValueIndex = 0);
end;

function InitializeSetup(): Boolean;
begin
  Result := True;
end;
