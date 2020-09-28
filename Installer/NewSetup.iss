; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "Project 1.27"
#define MyAppVersion "0.0.3.0"
#define MyAppPublisher "Project 1.27 Inc."
#define MyAppURL "https://github.com/TwosHusbandS/Project-127/blob/master/README.md"
#define MyAppExeName "Project 1.27.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{2E862DB9-ABA7-4F67-A954-4DF9D0349CAA}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
; Uncomment the following line to run in non administrative install mode (install for current user only.)
;PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
OutputDir=C:\Users\ingow\source\repos\Project-127\Installer
OutputBaseFilename=Project_127_Installer_V_0_0_3_0
SetupIconFile=C:\Users\ingow\source\repos\Project-127\Artwork\icon.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "C:\Users\ingow\source\repos\Project-127\bin\x64\Release\Project 1.27.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\ingow\source\repos\Project-127\bin\x64\Release\cef.pak"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\ingow\source\repos\Project-127\bin\x64\Release\cef_100_percent.pak"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\ingow\source\repos\Project-127\bin\x64\Release\cef_200_percent.pak"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\ingow\source\repos\Project-127\bin\x64\Release\cef_extensions.pak"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\ingow\source\repos\Project-127\bin\x64\Release\CefSharp.BrowserSubprocess.Core.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\ingow\source\repos\Project-127\bin\x64\Release\CefSharp.BrowserSubprocess.Core.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\ingow\source\repos\Project-127\bin\x64\Release\CefSharp.BrowserSubprocess.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\ingow\source\repos\Project-127\bin\x64\Release\CefSharp.BrowserSubprocess.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\ingow\source\repos\Project-127\bin\x64\Release\CefSharp.Core.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\ingow\source\repos\Project-127\bin\x64\Release\CefSharp.Core.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\ingow\source\repos\Project-127\bin\x64\Release\CefSharp.Core.xml"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\ingow\source\repos\Project-127\bin\x64\Release\CefSharp.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\ingow\source\repos\Project-127\bin\x64\Release\CefSharp.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\ingow\source\repos\Project-127\bin\x64\Release\CefSharp.Wpf.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\ingow\source\repos\Project-127\bin\x64\Release\CefSharp.Wpf.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\ingow\source\repos\Project-127\bin\x64\Release\CefSharp.Wpf.XML"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\ingow\source\repos\Project-127\bin\x64\Release\CefSharp.XML"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\ingow\source\repos\Project-127\bin\x64\Release\chrome_elf.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\ingow\source\repos\Project-127\bin\x64\Release\d3dcompiler_47.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\ingow\source\repos\Project-127\bin\x64\Release\debug.log"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\ingow\source\repos\Project-127\bin\x64\Release\devtools_resources.pak"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\ingow\source\repos\Project-127\bin\x64\Release\icudtl.dat"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\ingow\source\repos\Project-127\bin\x64\Release\libcef.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\ingow\source\repos\Project-127\bin\x64\Release\libEGL.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\ingow\source\repos\Project-127\bin\x64\Release\libGLESv2.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\ingow\source\repos\Project-127\bin\x64\Release\Project 1.27.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\ingow\source\repos\Project-127\bin\x64\Release\snapshot_blob.bin"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\ingow\source\repos\Project-127\bin\x64\Release\v8_context_snapshot.bin"; DestDir: "{app}"; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

