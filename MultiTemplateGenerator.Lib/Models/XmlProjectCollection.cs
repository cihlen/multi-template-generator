using System.Collections.Generic;

namespace MultiTemplateGenerator.Lib.Models
{
    public class XmlProjectCollection : XmlTemplateItemBase
    {
        public string SolutionTemplateFile { get; }

        public XmlProjectCollection(string solutionTemplateFile) : base("ProjectCollection")
        {
            SolutionTemplateFile = solutionTemplateFile;
        }

        public IEnumerable<XmlTemplateItemBase> ProjectTemplateLinks { get; set; } = new List<XmlTemplateItemBase>();
        public IEnumerable<XmlTemplateItemBase> SolutionFolders { get; set; } = new List<XmlTemplateItemBase>();
    }
}