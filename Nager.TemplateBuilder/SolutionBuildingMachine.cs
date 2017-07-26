using EnvDTE;
using EnvDTE80;
using log4net;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using Nager.TemplateBuilder.Model;
using VSLangProj;

namespace Nager.TemplateBuilder
{
    public class SolutionBuildingMachine : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SolutionBuildingMachine));
        private DTE2 _visualStudioInstance;

        public SolutionBuildingMachine()
        {
            var type = Type.GetTypeFromProgID("VisualStudio.DTE.15.0");
            var obj = Activator.CreateInstance(type, true);
            this._visualStudioInstance = (DTE2)obj;

            MessageFilter.Register();

            this._visualStudioInstance.UserControl = true;
            this._visualStudioInstance.MainWindow.Visible = true;
            this._visualStudioInstance.MainWindow.Activate();
        }

        public void Dispose()
        {
            this._visualStudioInstance.Quit();
            MessageFilter.Revoke();
        }

        public bool Build(SolutionInfo solutionInfo)
        {
            Log.Info($"{nameof(Build)} - Create Solution");
            if (!this.CreateSolution(solutionInfo))
            {
                return false;
            }

            Log.Info($"{nameof(Build)} - Create Projects");
            foreach (var projectInfo in solutionInfo.ProjectInfos)
            {
                Log.Info($"{nameof(Build)} - Create Project {projectInfo.Name}");
                this.CreateProject(projectInfo);
                this.ChangeFrameworkVersion(projectInfo);
                this.AddFolder(projectInfo);
                this.RemoveFiles(projectInfo);
                this.AddFiles(projectInfo);
                this.InstallNugetPackage(projectInfo);
                this.ChangeFile(projectInfo);
            }

            Log.Info($"{nameof(Build)} - Add Project References");
            foreach (var projectInfo in solutionInfo.ProjectInfos)
            {
                this.AddReferences(projectInfo);
            }

            return true;
        }

        #region Solution

        private bool CreateSolution(SolutionInfo solutionInfo)
        {
            try
            {
                if (this._visualStudioInstance.Solution.IsOpen)
                {
                    // Close the solution saving it
                    this._visualStudioInstance.Solution.Close(true);
                }

                // Get the folder where to save the solution
                var solutionFullFolder = Path.Combine(solutionInfo.Path, solutionInfo.Name);

                // Important: if the folder doesn't exist, create it. Otherwise Visual Studio 
                // doesn't create it and a prompt is shown when the SaveAs method is called
                if (!Directory.Exists(solutionFullFolder))
                {
                    Directory.CreateDirectory(solutionFullFolder);
                }

                // Create the solution
                this._visualStudioInstance.Solution.Create(solutionInfo.Path, solutionInfo.Name);

                // Compose the full name of the solution file
                var solutionFullFileName = Path.Combine(solutionInfo.Path, solutionInfo.Name, $"{solutionInfo.Name}.sln");

                // Save the solution
                this._visualStudioInstance.Solution.SaveAs(solutionFullFileName);

                return true;
            }
            catch (Exception exception)
            {
                Log.Error(nameof(CreateSolution), exception);
            }
            return false;
        }

        private bool SaveSolution()
        {
            this._visualStudioInstance.Solution.SaveAs(this._visualStudioInstance.Solution.FileName);
            return true;
        }

        #endregion

        #region Project

        private string GetTemplateFileFullName(ProjectTemplate projectTemplate)
        {
            var programfiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            string template;

            switch (projectTemplate)
            {
                case ProjectTemplate.WindowsClassicDesktopWindowsFormsApp:
                    template = @"Microsoft Visual Studio\2017\Community\Common7\IDE\ProjectTemplates\CSharp\Windows\1033\WindowsApplication\csWindowsApplication.vstemplate";
                    return Path.Combine(programfiles, template);
                case ProjectTemplate.WindowsClassicDesktopClassLibrary:
                    template = @"Microsoft Visual Studio\2017\Community\Common7\IDE\ProjectTemplates\CSharp\Windows\1033\ClassLibrary\csClassLibrary.vstemplate";
                    return Path.Combine(programfiles, template);
                case ProjectTemplate.WindowsClassicDesktopConsoleApp:
                    template = @"Microsoft Visual Studio\2017\Community\Common7\IDE\ProjectTemplates\CSharp\Windows\1033\ConsoleApplication\csConsoleApplication.vstemplate";
                    return Path.Combine(programfiles, template);
                case ProjectTemplate.NetStandardClassLibrary:
                    template = @"Microsoft Visual Studio\2017\Community\Common7\IDE\Extensions\bglh14rs.zbc\ProjectTemplates\CSharp\.NET Standard\1033\ClassLibrary\ClassLibrary.vstemplate";
                    return Path.Combine(programfiles, template);
            }

            return null;
        }

        private void CreateProject(ProjectInfo projectInfo)
        {
            try
            {
                var solution = (Solution2)this._visualStudioInstance.Solution;

                // Get the full name of the solution file
                var solutionFileFullName = solution.FileName;

                // Get the full name of the solution folder
                var solutionFolderFullName = Path.GetDirectoryName(solutionFileFullName);

                // Compose the full name of the project folder
                var projectFolderFullName = Path.Combine(solutionFolderFullName, projectInfo.Name);
                if (!(projectFolderFullName.EndsWith(@"\")))
                {
                    projectFolderFullName += @"\";
                }

                var projectTemplateFileName = this.GetTemplateFileFullName(projectInfo.Template);

                // Add the project
                solution.AddFromTemplate(projectTemplateFileName, projectFolderFullName, projectInfo.Name, false);

                //Save
                this.SaveSolution();
            }
            catch (Exception exception)
            {
                Log.Error(nameof(CreateProject), exception);
            }
        }

        private Project InternalGetProject(string name)
        {
            try
            {
                for (var i = 1; i <= this._visualStudioInstance.Solution.Projects.Count; i++)
                {
                    var project = this._visualStudioInstance.Solution.Projects.Item(i);
                    //var cleanName = this.GetCleanName(project.Name);

                    if (project.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        return project;
                    }
                }
            }
            catch (COMException exception)
            {
                Log.Debug("InternalGetProject", exception);
            }

            return null;
        }

        private Project GetProject(string name)
        {
            var retry = 5;

            while (retry > 0)
            {
                var project = this.InternalGetProject(name);
                if (project == null)
                {
                    retry--;
                    System.Threading.Thread.Sleep(1000);
                    continue;
                }

                return project;
            }

            return null;
        }

        private void ChangeFrameworkVersion(ProjectInfo projectInfo)
        {
            if (projectInfo.DotNetFrameworkVersion == null)
            {
                return;
            }

            var project = this.GetProject(projectInfo.Name);

            var currentVersion = project.Properties.Item("TargetFrameworkMoniker").Value;
            project.Properties.Item("TargetFrameworkMoniker").Value = new FrameworkName(".NETFramework", projectInfo.DotNetFrameworkVersion).FullName;

            //if (currentVersion.(".NETFramework,Version=v4.5"))
            //{

            //}
        }

        #endregion

        #region Folder

        private void AddFolder(ProjectInfo projectInfo)
        {
            if (projectInfo.Folders == null)
            {
                return;
            }

            var project = this.GetProject(projectInfo.Name);

            foreach (var folder in projectInfo.Folders)
            {
                project.ProjectItems.AddFolder(folder);
            }
        }

        #endregion

        #region Files

        private void RemoveFiles(ProjectInfo projectInfo)
        {
            if (projectInfo.RemoveFiles == null)
            {
                return;
            }

            var project = this.GetProject(projectInfo.Name);

            foreach (var removeFile in projectInfo.RemoveFiles)
            {
                for (var i = 1; i <= project.ProjectItems.Count; i++)
                {
                    var item = project.ProjectItems.Item(i);
                    if (item.Name == removeFile.Name)
                    {
                        item.Delete();
                    }
                }
            }

            this.SaveSolution();
        }

        private void AddFiles(ProjectInfo projectInfo)
        {
            if (projectInfo.Files == null)
            {
                return;
            }

            var project = this.GetProject(projectInfo.Name);

            foreach (var file in projectInfo.Files)
            {
                var projectItems = project.ProjectItems;

                if (!String.IsNullOrEmpty(file.Folder))
                {
                    for (var i = 1; i <= project.ProjectItems.Count; i++)
                    {
                        var item = project.ProjectItems.Item(i);
                        if (item.Name == file.Folder)
                        {
                            projectItems = item.ProjectItems;
                            break;
                        }
                    }
                }

                if (this.PrepareFile(file, projectInfo.Name))
                {
                    var projectItem = projectItems.AddFromFileCopy(Path.Combine(Environment.CurrentDirectory, "FileTemplate", file.Name));

                    #region CopyToOutputDirectory

                    if (file.CopyToOutputDirectory)
                    {
                        foreach (Property property in projectItem.Properties)
                        {
                            if (property.Name == "CopyToOutputDirectory")
                            {
                                property.Value = 2;
                            }
                        }
                    }

                    #endregion
                }
                else
                {
                    Log.Error($"{nameof(AddFiles)} - Cannot found file {file.Name}, check copy to output directory setting");
                }
            }

            this.SaveSolution();
        }

        private bool PrepareFile(ProjectFile file, string projectNamespace)
        {
            var templatePath = $@"FileTemplate\{file.Name}.txt";

            if (!File.Exists(templatePath))
            {
                return false;
            }

            var content = File.ReadAllText(templatePath);

            #region Add Namespace

            if (file.AddNamespaces != null)
            {
                var sb = new StringBuilder();
                foreach (var addNamespace in file.AddNamespaces)
                {
                    sb.AppendLine($"using {addNamespace};");
                }

                content = content.Insert(0, sb.ToString());
            }

            #endregion

            #region Replace Content

            if (file.Name.EndsWith(".cs"))
            {
                content = content.Replace("##projectname##", projectNamespace);
            }

            #endregion

            File.WriteAllText($@"FileTemplate\{file.Name}", content);

            return true;
        }

        private void ChangeFile(ProjectInfo projectInfo)
        {
            if (projectInfo.ChangeFile == null)
            {
                return;
            }

            var project = this.GetProject(projectInfo.Name);

            foreach (var file in projectInfo.ChangeFile)
            {
                var projectItems = project.ProjectItems;

                if (!String.IsNullOrEmpty(file.Folder))
                {
                    for (var i = 1; i <= project.ProjectItems.Count; i++)
                    {
                        var item = project.ProjectItems.Item(i);
                        if (item.Name == file.Folder)
                        {
                            projectItems = item.ProjectItems;
                            break;
                        }
                    }
                }

                switch (file.Operation)
                {
                    case ChangeFileOperation.Add:
                        var changeFileAdd = file as ChangeFileAdd;
                        this.ChangeFileAdd(projectItems, changeFileAdd);
                        break;
                    case ChangeFileOperation.Remove:
                        var changeFileRemove = file as ChangeFileRemove;
                        Log.Debug($"{nameof(ChangeFile)} - Operation remove not supported");
                        break;
                    case ChangeFileOperation.Replace:
                        var changeFileReplace = file as ChangeFileReplace;
                        Log.Debug($"{nameof(ChangeFile)} - Operation replace not supported");
                        break;
                    default:
                        break;
                }
            }
        }

        private void ChangeFileAdd(ProjectItems projectItems, ChangeFileAdd changeFile)
        {
            for (var i = 1; i <= projectItems.Count; i++)
            {
                var item = projectItems.Item(i);
                if (item.Name != changeFile.Name)
                {
                    continue;
                }

                var path = item.Properties.Item("FullPath").Value;

                if (changeFile.EndOfFile == true)
                {
                    using (var streamwriter = File.AppendText(path))
                    {
                        streamwriter.WriteLine(changeFile.Data);
                        streamwriter.Flush();
                        streamwriter.Close();
                    }
                    continue;
                }

                Log.Debug($"{nameof(ChangeFileAdd)} - Operation not supported");
            }
        }

        #endregion

        #region References

        private void AddReferences(ProjectInfo projectInfo)
        {
            if (projectInfo.ProjectReferences == null)
            {
                return;
            }

            var project = this.GetProject(projectInfo.Name);
            var vsProject = project.Object as VSProject;

            foreach (var reference in projectInfo.ProjectReferences)
            {
                var referenceProject = this.GetProject(reference.Name);
                vsProject.References.AddProject(referenceProject);
            }
        }

        #endregion

        #region Nuget

        private void InstallNugetPackage(ProjectInfo projectInfo)
        {
            if (projectInfo.NugetPackages == null)
            {
                return;
            }

            foreach (var nuget in projectInfo.NugetPackages)
            {
                this.ProcessInstallPackage(nuget, projectInfo);
            }
        }

        private void ProcessInstallPackage(string nugetPackage, ProjectInfo projectInfo)
        {
            var packageManagerConsoleGuid = "{0AD07096-BBA9-4900-A651-0598D26F6D24}";
            var window = this._visualStudioInstance.Windows.Item(packageManagerConsoleGuid);
            window.Activate();

            var commandName = "View.PackageManagerConsole";
            var nugetCommand = $"install-package {nugetPackage} -ProjectName {projectInfo.Name}";

            for (var retry = 5; retry != 0; retry--)
            {
                try
                {
                    this._visualStudioInstance.ExecuteCommand(commandName, nugetCommand);
                    break;
                }
                catch (COMException exception)
                {
                    System.Threading.Thread.Sleep(5000);
                    Log.Debug($"{nameof(ProcessInstallPackage)} - {nugetPackage}", exception);
                }
            }

            var successful = false;
            for (var retry = 30; retry != 0; retry--)
            {
                if (this.IsPackageInstalled(nugetPackage, projectInfo))
                {
                    successful = true;
                    break;
                }
                Log.Debug($"{nameof(ProcessInstallPackage)} - {nugetPackage} try again, retry:{retry}");
                System.Threading.Thread.Sleep(1000);
            }


            Log.Debug($"{nameof(ProcessInstallPackage)} - {nugetPackage} Successful:{successful}");
            System.Threading.Thread.Sleep(1000);
        }

        private bool IsPackageInstalled(string nugetPackage, ProjectInfo projectInfo)
        {
            var project = this.GetProject(projectInfo.Name);

            var path = Path.GetDirectoryName(project.FullName);
            if (!File.Exists($@"{path}\packages.config"))
            {
                return false;
            }

            try
            {
                using (var file = File.Open($@"{path}\packages.config", FileMode.Open, FileAccess.Read))
                {
                    var data = new byte[file.Length];
                    file.Read(data, 0, (int)file.Length);
                    file.Close();

                    var result = Encoding.UTF8.GetString(data);
                    if (result.Contains(nugetPackage))
                    {
                        return true;
                    }
                }
            }
            catch (Exception exception)
            {
                Log.Debug(nameof(IsPackageInstalled), exception);
            }

            return false;
        }

        #endregion
    }
}
