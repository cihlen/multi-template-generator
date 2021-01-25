using System;
using System.Diagnostics;
using System.IO;
using MultiTemplateGenerator.Lib;
using MultiTemplateGenerator.Lib.Models;
using MultiTemplateGenerator.UI.Helpers;

namespace MultiTemplateGenerator.UI.Models
{
    public static class AppSettings
    {
        private static readonly string SolutionSettingsFile;

        static AppSettings()
        {
            SolutionSettingsFile = Path.Combine(Path.GetTempPath(), "SolutionTemplate.json");

            if (SolutionSettingsFile.FileExists())
            {
                try
                {
                    SolutionTemplateSettings = JsonFileHelper.ReadJsonFile<ProjectTemplate>(SolutionSettingsFile);
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
            JsonFileHelper.SaveJsonFile(SolutionTemplateSettings, SolutionSettingsFile);
        }

    }
}
