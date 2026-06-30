; ONAIR — Inno Setup орнатушы скрипті
; Жинау: ISCC.exe installer.iss  →  Desktop\ONAIR-Setup.exe

[Setup]
AppId={{2C9B1E7A-4D3F-4A21-9E55-0A1B2C3D4E5F}}
AppName=ONAIR
AppVersion=1.0
AppPublisher=Jambyl
AppPublisherURL=https://github.com/jambyylf/onair
DefaultDirName={autopf}\ONAIR
DefaultGroupName=ONAIR
DisableProgramGroupPage=yes
AllowNoIcons=yes
OutputDir=C:\Users\FALCON\Desktop
OutputBaseFilename=ONAIR-Setup
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
SetupIconFile=C:\Users\FALCON\ONAIR\onair.ico
UninstallDisplayIcon={app}\ONAIR.exe
PrivilegesRequired=admin

[Languages]
Name: "en"; MessagesFile: "compiler:Default.isl"
Name: "ru"; MessagesFile: "compiler:Languages\Russian.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
Source: "C:\Users\FALCON\Desktop\ONAIR-Portable\*"; DestDir: "{app}"; \
  Excludes: "android-device.txt,settings.txt,phones.txt,needed-dlls.txt,ONAIR.ps1,*.mp4,annot_*.png"; \
  Flags: recursesubdirs createallsubdirs ignoreversion

[Icons]
Name: "{group}\ONAIR"; Filename: "{app}\ONAIR.exe"
Name: "{group}\{cm:UninstallProgram,ONAIR}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\ONAIR"; Filename: "{app}\ONAIR.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\ONAIR.exe"; Description: "{cm:LaunchProgram,ONAIR}"; Flags: nowait postinstall skipifsilent
