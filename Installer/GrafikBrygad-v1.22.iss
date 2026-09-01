; ============================================================
; Grafik Brygad VEOLIA Energia Łódź
; Instalator Inno Setup
; Wersja programu: 1.22
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
;     GrafikBrygad-v1.22.iss   <-- ten plik
;
; WAŻNE:
; AppId pozostaje IDENTYCZNY jak w poprzednich wersjach.
; Dzięki temu instalator v1.22 aktualizuje istniejącą instalację.
;
; Skrót na pulpicie:
; "Grafik Brygad"
; ============================================================

#define MyAppName "Grafik Brygad VEOLIA Energia Łódź"
#define MyAppVersion "1.22"
#define MyAppPublisher "Marek Walaszczyk"
#define MyAppExeName "GrafikBrygad.exe"
#define MyDesktopShortcutName "Grafik Brygad"

[Setup]
AppId={{E4A28DA6-6E96-4B16-9B8E-6E7F8E143A19}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} v{#MyAppVersion}
AppPublisher={#MyAppPublisher}

; Instalacja dla bieżącego użytkownika - bez wymagania konta administratora.
PrivilegesRequired=lowest
DefaultDirName={localappdata}\Programs\GrafikBrygad
DisableDirPage=auto

; Menu Start.
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes

; Program jest publikowany jako win-x64.
SetupArchitecture=x64

; Ikony.
SetupIconFile=..\Images\GrafikBrygad.ico
UninstallDisplayIcon={app}\{#MyAppExeName}

; Plik wynikowy instalatora.
OutputDir=Output
OutputBaseFilename=GrafikBrygad-v1.22-Setup

; Metadane pliku Setup.exe.
VersionInfoVersion=1.22.0.0
VersionInfoProductVersion=1.22
VersionInfoProductName={#MyAppName}
VersionInfoDescription=Instalator {#MyAppName}
VersionInfoCompany={#MyAppPublisher}

; Wygląd i kompresja.
WizardStyle=modern
Compression=lzma2/max
SolidCompression=yes

; Dodatkowe ustawienia.
SetupLogging=yes
CloseApplications=yes
RestartApplications=no

[Languages]
Name: "polish"; MessagesFile: "compiler:Languages\Polish.isl"

[Tasks]
; Domyślnie zaznaczone - użytkownik może odznaczyć.
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
; Kopiuje całą publikację Self-contained win-x64 razem z podfolderami.
Source: "..\publish\win-x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
; Skrót w menu Start zachowuje pełną nazwę programu.
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; IconFilename: "{app}\{#MyAppExeName}"

; Skrót na pulpicie ma krótką nazwę.
Name: "{autodesktop}\{#MyDesktopShortcutName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; IconFilename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
; Opcja uruchomienia programu na ostatniej stronie instalatora.
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#MyAppName}}"; Flags: nowait postinstall skipifsilent
