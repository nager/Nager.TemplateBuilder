using System.Collections.Generic;

namespace Nager.TemplateBuilder.Model
{
    public class SolutionInfo
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public List<ProjectInfo> ProjectInfos { get; set; }

        public SolutionInfo(string name, string path)
        {
            this.Name = name;
            this.Path = path;
            this.ProjectInfos = new List<ProjectInfo>();
        }
    }
}
