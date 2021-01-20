using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiTemplateGenerator.Lib;
using MultiTemplateGenerator.Lib.Models;
using MultiTemplateGenerator.UI.Helpers;
using Serilog.Core;

namespace MultiTemplateGenerator.UI.Models
{
    public static class AppSettings
    {
        private static readonly string _solutionSettingsFile;

        static AppSettings()
        {
            _solutionSettingsFile = Path.Combine(Path.GetTempPath(), "SolutionTemplate.json");
            
            if (_solutionSettingsFile.FileExists())
            {
                try
                {
                    SolutionTemplateSettings = JsonFileHelper.ReadJsonFile<ProjectTemplate>(_solutionSettingsFile);
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e);
                }
            }

            if (SolutionTemplateSettings == null)
            {
                SolutionTemplateSettings = new ProjectTemplate
                {
                    TemplateName = "My Multi-Template Solution",
                    LanguageTag = "C#",
                    PlatformTags = "Windows",
                    ProjectTypeTags = "Web"
                };
            }
        }
        public static IProjectTemplate SolutionTemplateSettings { get; }

        public static void SaveSettings()
        {
            JsonFileHelper.SaveJsonFile(SolutionTemplateSettings, _solutionSettingsFile);
        }

    }
}
