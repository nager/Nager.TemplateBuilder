using System;
using System.Collections.Generic;

namespace Nager.TemplateBuilder.Model
{
    public class ProjectInfo
    {
        public string Name { get; set; }
        public ProjectTemplate Template { get; set; }
        public List<string> NugetPackages { get; set; }
        public List<ProjectInfo> ProjectReferences { get; set; }
        public List<string> Folders { get; set; }
        public List<ProjectFile> Files { get; set; }
        public List<ProjectFile> RemoveFiles { get; set; }
        public Version DotNetFrameworkVersion { get; set; }
        public List<string> ChangeFile { get; set; }

        public ProjectInfo(string name, ProjectTemplate template)
        {
            this.Name = name;
            this.Template = template;
        }
    }
}
