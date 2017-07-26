namespace Nager.TemplateBuilder.Model
{
    public class ChangeFileAdd : ChangeFile
    {
        public int? LineNumber { get; private set; }
        public bool? EndOfFile { get; private set; }
        public string Data { get; private set; }

        public ChangeFileAdd(string name, string data)
        {
            base.Operation = ChangeFileOperation.Add;
            base.Name = name;
            this.EndOfFile = true;
            this.Data = data;
        }

        public ChangeFileAdd(string name, string folder, string data)
        {
            base.Operation = ChangeFileOperation.Add;
            base.Name = name;
            base.Folder = folder;
            this.EndOfFile = true;
            this.Data = data;
        }

        public ChangeFileAdd(string name, int lineNumber, string data)
        {
            base.Operation = ChangeFileOperation.Add;
            base.Name = name;
            this.LineNumber = lineNumber;
            this.Data = data;
        }

        public ChangeFileAdd(string name, string folder, int lineNumber, string data)
        {
            base.Operation = ChangeFileOperation.Add;
            base.Name = name;
            base.Folder = folder;
            this.LineNumber = lineNumber;
            this.Data = data;
        }
    }
}
