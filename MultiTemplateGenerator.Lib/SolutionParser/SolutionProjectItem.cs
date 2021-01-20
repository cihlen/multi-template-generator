using System;
using System.Collections.Generic;

namespace MultiTemplateGenerator.Lib.SolutionParser
{
    public class SolutionProjectItem
    {
        public string ProjectFileName { get; }

        public const string SolutionFolderGuid = "{2150E333-8FDC-42A3-9474-1A3956D46DE8}";

        public bool IsProject => !SolutionFolderGuid.Equals(TypeUid);
        //string.IsNullOrWhiteSpace(ProjectFileName) ||
        //                         ProjectFileName.EndsWith(".csproj", StringComparison.InvariantCultureIgnoreCase);

        public string TypeUid { get; set; }

        public string Uid { get; set; }
        public string Name { get; set; }

        public SolutionProjectItem Parent { get; set; }
        public List<SolutionProjectItem> Children { get; set; } = new List<SolutionProjectItem>();

        public SolutionProjectItem(string projectLine)
        {
            var parts = projectLine.Split(new[] { "\", \"" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3)
                return;

            Name = parts[0].Substring(parts[0].IndexOf(" = ") + 4).Trim(new[] { '\"' });

            var pos = parts[0].IndexOf("{");
            TypeUid = parts[0].Substring(pos, parts[0].IndexOf("}") - pos + 1).Trim(new[] { '\"' });

            ProjectFileName = parts[1].Trim(new[] { '\"' });
            Uid = parts[2].Trim(new[] { '\"' });
        }

        public override string ToString()
        {
            if (Parent != null)
                return $"{Parent.Name}\\{Name} Uid: {Uid}";
            return $"{Name} Uid: {Uid}";
        }
    }
}