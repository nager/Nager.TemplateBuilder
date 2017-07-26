namespace Nager.TemplateBuilder.Model
{
    public class ChangeFileReplace : ChangeFile
    {
        public string Search { get; private set; }
        public string Replace { get; private set; }

        public ChangeFileReplace(string name, string search, string replace)
        {
            base.Operation = ChangeFileOperation.Replace;
            base.Name = name;

            this.Search = search;
            this.Replace = replace;
        }

        public ChangeFileReplace(string name, string folder, string search, string replace)
        {
            base.Operation = ChangeFileOperation.Replace;
            base.Name = name;
            base.Folder = folder;

            this.Search = search;
            this.Replace = replace;
        }
    }
}
