Nager.TemplateBuilder
==========

Create automated Visual Studio Solutions and Projects

### Features
- [x] Project references
- [x] Nuget references
- [x] Add Folders
- [x] Add Files
- [x] Choose Framework Version

##### Example
```cs
var libraryProject = new ProjectInfo("DemoProject.Library", ProjectTemplate.WindowsClassicDesktopClassLibrary);
libraryProject.RemoveFiles = new List<ProjectFile> { new ProjectFile("Class1.cs") };
libraryProject.DotNetFrameworkVersion = new Version(4, 6, 1);
//libraryProject.ProjectReferences = new List<ProjectInfo> { otherProject };
libraryProject.NugetPackages = new List<string> { "log4net", "Mapster", "System.ServiceModel.NetTcp", "System.Runtime.Serialization.Xml" };
libraryProject.Folders = new List<string>() { "WCF", "Core", "Contract", "Model" };
libraryProject.Files = new List<ProjectFile>();
libraryProject.Files.Add(new ProjectFile("Controller.cs"));
libraryProject.Files.Add(new ProjectFile("SystemCore.cs", "Core") { AddNamespaces = new List<string> { "OtherProject.Common.WCF", "OtherProject.Common.Factory" } });
libraryProject.Files.Add(new ProjectFile("CommunicationService.cs", "WCF") { AddNamespaces = new List<string> { "OtherProject.Common.Model", "OtherProject.Common.WCF" } });
libraryProject.ChangeFile = new List<string> { "[assembly: log4net.Config.XmlConfigurator(ConfigFile = \"log4net.config\", Watch = true)]" };
```

