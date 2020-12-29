using System;
using System.Collections.Generic;
using System.Linq;
using MultiTemplateGeneratorLib.Extensions;

namespace MultiTemplateGeneratorLib.Models
{
    public class SolutionItem
    {
        public string FileName { get; }

        public bool IsProject => string.IsNullOrWhiteSpace(FileName) ||
                                 FileName.EndsWith(".csproj", StringComparison.InvariantCultureIgnoreCase);

        public string TypeUid { get; set; }

        public string Uid { get; set; }
        public string Name { get; set; }

        public string TemplateFileName { get; set; }

        public string DefaultTemplateFileName => Name.GetSafeFileName() + ".vstemplate";
        public string DisplayName => Parent != null ? $"{Parent.Name} \\ {Name}" : Name;

        public SolutionItem Parent { get; set; }
        public List<SolutionItem> Children { get; set; } = new List<SolutionItem>();

        public SolutionItem(string name, string templateFileName)
        {
            Name = name;
            TemplateFileName = templateFileName;
            FileName = string.IsNullOrEmpty(templateFileName) ? Name : null;
        }

        public SolutionItem(string projectLine)
        {
            var parts = projectLine.Split(new[] { "\", \"" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3)
                return;

            Name = parts[0].Substring(parts[0].IndexOf(" = ") + 4).Trim(new[] { '\"' });

            var pos = parts[0].IndexOf("{");
            TypeUid = parts[0].Substring(pos, parts[0].IndexOf("}") - pos + 1).Trim(new[] { '\"' });

            FileName = parts[1].Trim(new[] { '\"' });

            Uid = parts[2].Trim(new[] { '\"' });
        }

        public override string ToString()
        {
            return $"{Name} Uid: {Uid}";
        }
    }

    public class SolutionItemCollection : List<SolutionItem>
    {
        public SolutionItemCollection(List<SolutionItem> items, Dictionary<string, string> nestedItems)
        {
            AddRange(items);

            NestedItems = nestedItems;
        }

        public Dictionary<string, string> NestedItems { get; set; }

        public List<SolutionItem> GetSortedHierarchy()
        {
            var list = new List<SolutionItem>(this);

            foreach (var item in list)
            {
                item.Children.Clear();
            }

            foreach (var nestedItem in NestedItems)
            {
                var child = list.Single(x => x.Uid.Equals(nestedItem.Key));
                var parent = list.Single(x => x.Uid.Equals(nestedItem.Value));
                parent.Children.Add(child);
                child.Parent = parent;
                list.Remove(child);
            }

            return list.OrderBy(x => x.Name).ToList();
        }
    }
}
