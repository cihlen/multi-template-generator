using System.Collections.Generic;
using System.IO;
using MultiTemplateGenerator.Lib.Models;

namespace MultiTemplateGenerator.Lib.Generator
{
    public class TemplateOptions
    {
        public IProjectTemplate SolutionTemplate { get; set; }
        public IEnumerable<IProjectTemplate> ProjectTemplates { get; set; }
        public string SolutionFolder { get; set; }
        public string TargetFolder { get; set; }
        public string TargetTemplatePath => !string.IsNullOrWhiteSpace(TargetFolder) ? Path.Combine(TargetFolder, $"{SolutionTemplate.TemplateFileName}") : string.Empty;
        public bool AutoImportToVS { get; set; }

        public override string ToString()
        {
            return $"{nameof(TargetTemplatePath)}: {TargetTemplatePath}\r\n{nameof(TargetFolder)}: {TargetFolder}\r\n{nameof(AutoImportToVS)}: {AutoImportToVS}";
        }
    }
}
