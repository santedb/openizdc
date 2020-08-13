; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "OpenIZ Disconnected Client SDK"
#define MyAppPublisher "Mohawk College of Applied Arts and Technology"
#define MyAppURL "http://openiz.org"
[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{6AB504BB-8E44-43E6-A5C7-E033DC8D819F}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={pf}\Mohawk College\OpenIZ\sdk
DisableProgramGroupPage=yes
LicenseFile=..\License.rtf
OutputDir=.\dist
OutputBaseFilename=openizdc-sdk-{#MyAppVersion}
Compression=lzma
SolidCompression=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: ".\bin\SignedRelease\Antlr3.Runtime.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\SignedRelease\ExpressionEvaluator.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\SignedRelease\esprima.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\SignedRelease\jint.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\SignedRelease\MARC.HI.EHRS.SVC.Auditing.Core.dll"; DestDir: "{app}"; Flags: ignoreversion
;Source: ".\bin\SignedRelease\Microsoft.VisualStudio.QualityTools.UnitTestFramework.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\SignedRelease\MohawkCollege.Util.Console.Parameters.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\SignedRelease\Mono.Data.Sqlite.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\SignedRelease\Newtonsoft.Json.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\SignedRelease\OpenIZ.BusinessRules.JavaScript.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\SignedRelease\OpenIZ.Core.Alert.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\SignedRelease\OpenIZ.Core.Applets.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\SignedRelease\OpenIZ.Core.Model.AMI.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\SignedRelease\OpenIZ.Core.Model.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\SignedRelease\OpenIZ.Core.Model.RISI.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\SignedRelease\OpenIZ.Core.Model.ViewModelSerializers.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\SignedRelease\OpenIZ.Core.PCL.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\SignedRelease\OpenIZ.Messaging.AMI.Client.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\SignedRelease\OpenIZ.Messaging.IMSI.Client.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\SignedRelease\OpenIZ.Messaging.RISI.Client.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\SignedRelease\OpenIZ.Mobile.Core.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\SignedRelease\OpenIZ.Mobile.Core.Xamarin.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\SignedRelease\OpenIZ.Mobile.Reporting.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\SignedRelease\OpenIZ.Protocol.Xml.dll"; DestDir: "{app}"; Flags: ignoreversion
;Source: ".\bin\SignedRelease\OpenIZ.Protocol.Xml.Test.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\SignedRelease\SharpCompress.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\SignedRelease\SQLite.Net.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\SignedRelease\SQLite.Net.Platform.Generic.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\SignedRelease\sqlite3.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\SignedRelease\System.Data.Portable.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\SignedRelease\System.Transactions.Portable.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\SignedRelease\zlibnet.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\SignedRelease\zxing.portable.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\SignedRelease\sqlite3.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\SignedRelease\minims.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\BrainBug\bin\SignedRelease\brainbug.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\BrainBug\bin\SignedRelease\SharpCompress.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\BrainBug\bin\SignedRelease\zlibnet.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\BrainBug\bin\SignedRelease\brainbug.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\BrainBug\bin\SignedRelease\SharpCompress.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\BrainBug\bin\SignedRelease\zlibnet.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\AppletCompiler\bin\Release\AppletCompiler.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\AppletCompiler\bin\Release\AjaxMin.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\AppletCompiler\bin\Release\OpenIZ.Core.Applets.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\AppletCompiler\bin\Release\OpenIZ.Core.Model.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\OpenIZ\bin\Release\LogViewer.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\OpenIZMobile\Assets\Applets\org.openiz.core.pak"; DestDir: "{app}"; Flags: recursesubdirs
Source: "..\OpenIZMobile\Assets\Applets\org.openiz.templates.pak"; DestDir: "{app}"; Flags: recursesubdirs
Source: ".\bin\SignedRelease\tools\cmdprompt.cmd"; DestDir: "{app}"; Flags: recursesubdirs
Source: "..\..\OpenIZ\bin\Release\Schema\*.xsd"; DestDir: "{app}\schema"; Flags: ignoreversion
Source: "..\OizDebug\bin\Release\OizDebug.exe"; DestDir: "{app}"; Flags: ignoreversion;


[Icons]
Name: "{commonprograms}\Open Immunize\SDK\OpenIZ Log Viewer"; Filename: "{app}\LogViewer.exe"
Name: "{commonprograms}\Open Immunize\SDK\OpenIZ SDK Command Prompt"; FileName:cmd; Parameters:"/k """"{app}\cmdprompt.cmd"" ""{app}"""""
Name: "{commonprograms}\Open Immunize\SDK\Business Rule Debugger"; Filename: cmd; Parameters: "/T:F0 /c """"{app}\oizdebug.exe"" --db=%localappdata%\MINIMS\OpenIZ.sqlite --bre""" 
Name: "{commonprograms}\Open Immunize\SDK\Clinical Protocol Debugger"; Filename: cmd; Parameters: "/T:F0 /c """"{app}\oizdebug.exe"" --db=%localappdata%\MINIMS\OpenIZ.sqlite --xprot"" "

