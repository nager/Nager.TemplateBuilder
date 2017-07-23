using System.Collections.Generic;

namespace Nager.TemplateBuilder.Model
{
    public class ProjectFile
    {
        public string Name { get; set; }
        public string Folder { get; set; }
        public bool CopyToOutputDirectory { get; set; }
        public List<string> AddNamespaces { get; set; }

        public ProjectFile(string name)
        {
            this.Name = name;
        }

        public ProjectFile(string name, string folder)
        {
            this.Name = name;
            this.Folder = folder;
        }
    }
}
