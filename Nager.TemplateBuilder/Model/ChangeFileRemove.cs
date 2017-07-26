namespace Nager.TemplateBuilder.Model
{
    public class ChangeFileRemove : ChangeFile
    {
        public int LineNumber { get; private set; }

        public ChangeFileRemove(string name, int lineNumber)
        {
            base.Operation = ChangeFileOperation.Remove;
            base.Name = name;

            this.LineNumber = lineNumber;
        }

        public ChangeFileRemove(string name, string folder, int lineNumber)
        {
            base.Operation = ChangeFileOperation.Remove;
            base.Name = name;
            base.Folder = folder;

            this.LineNumber = lineNumber;
        }
    }
}
