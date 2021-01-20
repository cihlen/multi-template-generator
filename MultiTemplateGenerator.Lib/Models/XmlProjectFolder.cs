using System.Collections.Generic;

namespace MultiTemplateGenerator.Lib.Models
{
    public class XmlProjectFolder : XmlTemplateItemBase
    {
        public XmlProjectFolder() : base("Folder")
        {
        }

        public string Name { get; set; }
        public string TargetFolderName { get; set; }
        public IEnumerable<XmlTemplateItemBase> Children { get; set; } = new List<XmlTemplateItemBase>();
    }
}