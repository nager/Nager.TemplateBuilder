namespace Nager.TemplateBuilder.Model
{
    public class ChangeFile
    {
        public string Name { get; protected set; }
        public string Folder { get; protected set; }
        public ChangeFileOperation Operation { get; protected set; }
    }
}
