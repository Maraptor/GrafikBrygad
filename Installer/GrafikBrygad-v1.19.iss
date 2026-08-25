; ============================================================
; Grafik Brygad VEOLIA Energia Łódź
; Instalator Inno Setup
; Wersja programu: 1.19
;
; Oczekiwana struktura katalogów projektu:
;
; GrafikBrygad\
;   Images\
;     GrafikBrygad.ico
;   publish\
;     win-x64\
;       GrafikBrygad.exe
;       ...pozostałe pliki publikacji...
;       Images\
;         veolia.png
;   Installer\
;     GrafikBrygad-v1.19.iss   <-- ten plik
;
; WAŻNE:
; AppId musi pozostać IDENTYCZNY w przyszłych wersjach,
; aby v1.20, v1.21 itd. aktualizowały istniejącą instalację.
; ============================================================

#define MyAppName "Grafik Brygad VEOLIA Energia Łódź"
#define MyAppVersion "1.19"
#define MyAppPublisher "Marek Walaszczyk"
#define MyAppExeName "GrafikBrygad.exe"

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

; Skróty i menu Start.
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes

; Program jest publikowany jako win-x64.
SetupArchitecture=x64

; Ikony.
SetupIconFile=..\Images\GrafikBrygad.ico
UninstallDisplayIcon={app}\{#MyAppExeName}

; Plik wynikowy instalatora.
OutputDir=Output
OutputBaseFilename=GrafikBrygad-v1.19-Setup

; Metadane pliku Setup.exe.
VersionInfoVersion=1.19.0.0
VersionInfoProductVersion=1.19
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
; Kopiuje CAŁĄ publikację Self-contained win-x64 razem z podfolderami,
; w tym Images\veolia.png.
Source: "..\publish\win-x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
; Skrót w menu Start.
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; IconFilename: "{app}\{#MyAppExeName}"

; Skrót na pulpicie - zależny od zadania "desktopicon".
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; IconFilename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
; Opcja uruchomienia programu na ostatniej stronie instalatora.
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#MyAppName}}"; Flags: nowait postinstall skipifsilent
