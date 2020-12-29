using System.Collections.Generic;
using MultiTemplateGeneratorLib.Extensions;

namespace MultiTemplateGeneratorLib.Models
{
    public class TemplateOptions
    {
        public string Name { get; set; }

        public string DefaultTemplateFileName => Name.GetSafeFileName() + ".vstemplate";
        public string Description { get; set; }
        public string DefaultName { get; set; }
        public string ProjectType { get; set; } = "CSharp";
        public string LanguageTag { get; set; } = "C#";
        public List<string> PlatformTags { get; set; } = new List<string> { "Windows" };
        public List<string> ProjectTypeTags { get; set; } = new List<string> { "Web" };
        public string ProjectSubType { get; set; }
        public string Icon { get; set; }
        public string PreviewImage { get; set; }
        public bool HiddenProjects { get; set; }
        public string DestinationFolder { get; set; }
        public bool UseSolution { get; set; }
        public bool Import { get; set; }
    }
}
