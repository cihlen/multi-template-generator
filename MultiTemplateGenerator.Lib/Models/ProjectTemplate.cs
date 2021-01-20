using System.Collections.Generic;

namespace MultiTemplateGenerator.Lib.Models
{
    public interface IProjectTemplate
    {
        string ProjectFileName { get; set; }
        string TemplateFileName { get; set; }

        /* REQUIRED */
        string TemplateName { get; set; }
        /* REQUIRED */
        string Description { get; set; }
        /* REQUIRED */
        string ProjectType { get; }
        /* REQUIRED */
        string IconImagePath { get; set; }

        string DefaultName { get; set; }
        string LanguageTag { get; set; }
        string PlatformTags { get; set; }
        string ProjectTypeTags { get; set; }
        string ProjectSubType { get; set; }
        string PreviewImagePath { get; set; }
        bool CreateNewFolder { get; set; }
        bool IsHidden { get; set; }

        bool IsProject { get; }
        List<IProjectTemplate> Children { get; }
        bool ProvideDefaultName { get; set; }
        bool CreateInPlace { get; set; }
        bool EnableLocationBrowseButton { get; set; }
        LocationFieldType LocationField { get; set; }
        string MaxFrameworkVersion { get; set; }
        string RequiredFrameworkVersion { get; set; }
        string FrameworkVersion { get; set; }
        
        int SortOrder { get; set; }

        //List<XmlTemplateItemBase> ProjectItems { get; set; }
    }

    public class ProjectTemplate : IProjectTemplate
    {
        private string _projectFileName;
        private string _templateName;

        public ProjectTemplate()
        {
        }

        public ProjectTemplate(bool isProject)
        {
            IsProject = isProject;
        }

        public string ProjectFileName
        {
            get => _projectFileName;
            set
            {
                _projectFileName = value;
                if (!string.IsNullOrWhiteSpace(_projectFileName) && string.IsNullOrEmpty(LanguageTag))
                    LanguageTag = _projectFileName.GetLanguageTagFromExtension();
            }
        }

        public string TemplateFileName { get; set; }

        public string TemplateName
        {
            get => _templateName;
            set
            {
                _templateName = value;

                TemplateFileName = TemplateName.GetSafeFileName() + ".vstemplate";
            }
        }

        public string Description { get; set; } = "<No description available>";
        public string ProjectType => ProjectFileName.GetProjectTypeFromExtension() ?? LanguageTag.GetProjectTypeFromExtension();

        public string DefaultName { get; set; }
        public string LanguageTag { get; set; }

        public string PlatformTags { get; set; }
        public string ProjectTypeTags { get; set; }
        public string ProjectSubType { get; set; }
        public string IconImagePath { get; set; }
        public string PreviewImagePath { get; set; }
        public bool CreateNewFolder { get; set; } = true;
        public bool IsHidden { get; set; }
        public bool IsProject { get; }
        public List<IProjectTemplate> Children { get; } = new List<IProjectTemplate>();
        public bool ProvideDefaultName { get; set; } = true;
        public bool CreateInPlace { get; set; } = true;
        public bool EnableLocationBrowseButton { get; set; } = true;
        public LocationFieldType LocationField { get; set; } = LocationFieldType.Enabled;
        public string MaxFrameworkVersion { get; set; }
        public string RequiredFrameworkVersion { get; set; }
        public string FrameworkVersion { get; set; }
        public int SortOrder { get; set; } = 1000;

        public override string ToString()
        {
            return (IsProject ? "Proj: " : "SolFolder: ") +  TemplateName;
        }
    }
}