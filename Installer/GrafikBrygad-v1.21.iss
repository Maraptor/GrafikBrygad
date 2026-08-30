; ============================================================
; Grafik Brygad VEOLIA Energia Łódź
; Instalator Inno Setup
; Wersja programu: 1.21
;
; Oczekiwana struktura katalogów projektu:
;
; GrafikBrygad\
;   Images\
;     GrafikBrygad.ico
;     veolia.png
;   publish\
;     win-x64\
;       GrafikBrygad.exe
;       ...pozostałe pliki publikacji...
;       Images\
;         veolia.png
;   Installer\
;     GrafikBrygad-v1.21.iss   <-- ten plik
;
; WAŻNE:
; AppId pozostaje IDENTYCZNY jak w poprzednich wersjach.
; Dzięki temu instalator v1.21 aktualizuje istniejącą instalację.
;
; Skrót na pulpicie ma krótką nazwę:
; "Grafik Brygad"
; ============================================================

#define MyAppName "Grafik Brygad VEOLIA Energia Łódź"
#define MyAppVersion "1.21"
#define MyAppPublisher "Marek Walaszczyk"
#define MyAppExeName "GrafikBrygad.exe"
#define MyDesktopShortcutName "Grafik Brygad"

[Setup]
AppId={{E4A28DA6-6E96-4B16-9B8E-6E7F8E143A19}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} v{#MyAppVersion}
AppPublisher={#MyAppPublisher}

PrivilegesRequired=lowest
DefaultDirName={localappdata}\Programs\GrafikBrygad
DisableDirPage=auto

DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes

SetupArchitecture=x64

SetupIconFile=..\Images\GrafikBrygad.ico
UninstallDisplayIcon={app}\{#MyAppExeName}

OutputDir=Output
OutputBaseFilename=GrafikBrygad-v1.21-Setup

VersionInfoVersion=1.21.0.0
VersionInfoProductVersion=1.21
VersionInfoProductName={#MyAppName}
VersionInfoDescription=Instalator {#MyAppName}
VersionInfoCompany={#MyAppPublisher}

WizardStyle=modern
Compression=lzma2/max
SolidCompression=yes

SetupLogging=yes
CloseApplications=yes
RestartApplications=no

[Languages]
Name: "polish"; MessagesFile: "compiler:Languages\Polish.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
Source: "..\publish\win-x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; IconFilename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyDesktopShortcutName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; IconFilename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#MyAppName}}"; Flags: nowait postinstall skipifsilent
