using System.Collections.Generic;

namespace MultiTemplateGenerator.Lib.Models
{
    public class XmlSolutionFolder : XmlTemplateItemBase
    {
        public XmlSolutionFolder() : base("SolutionFolder")
        {
        }

        public string Name { get; set; }
        public IEnumerable<XmlTemplateItemBase> Children { get; set; } = new List<XmlTemplateItemBase>();
    }
}