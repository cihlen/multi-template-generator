using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using MultiTemplateGenerator.Lib.Models;

namespace MultiTemplateGenerator.Lib
{
    public static class MultiTemplateExtensions
    {
        private static readonly WildcardComparer WildcardComparer = new WildcardComparer();

        public static string GetProjectTypeFromExtension(this string filename)
        {
            var ext = string.IsNullOrWhiteSpace(filename) ? string.Empty : Path.GetExtension(filename).ToLower();
            switch (ext)
            {
                case ".wapproj":
                case ".csproj": return "CSharp";
                case ".vcxproj": return "Cpp";
                case ".vbproj":
                case ".vbp":
                    return "VisualBasic";
                case ".fsproj": return "fsharp";
                case ".njsproj": return "JavaScript";
                case ".java": return "Java";
                case ".sqlproj": return "QueryLanguage";
                case ".pyproj": return "Python";
                default: return "CSharp";
            }
        }

        public static string GetLanguageTagFromExtension(this string filename)
        {
            var ext = string.IsNullOrWhiteSpace(filename) ? string.Empty : Path.GetExtension(filename).ToLower();
            switch (ext)
            {
                case ".wapproj":
                case ".csproj": return "C#";
                case ".vcxproj": return "C++";
                case ".vbproj":
                case ".vbp":
                    return "Visual Basic";
                case ".fsproj": return "F#";
                case ".njsproj": return "JavaScript";
                case ".sqlproj": return "Query Language";
                case ".pyproj": return "Python";
                default: return null;
            }
        }

        public static string GetTemplateFolderNameByLanguage(this string languageTag)
        {
            if (string.IsNullOrWhiteSpace(languageTag))
                return string.Empty;

            switch (languageTag)
            {
                case "C#": return "Visual C#";
                case "C++": return "Visual C++ Project";
                case "JAVASCRIPT": return "JavaScript";
                case "TYPESCRIPT": return "TypeScript";
                case "VISUAL BASIC": return "Visual Basic";
                default: return string.Empty;
            }
        }

        //public static string GetProjectTypeFromLanguageTag(this string languageTag)
        //{
        //    languageTag = languageTag.ToUpper();
        //    switch (languageTag)
        //    {
        //        case ".wapproj":
        //        case "C#": return "CSharp";
        //        case "C++": return "Cpp";
        //        case "F#": return "fsharp";
        //        case "JAVA": return "java";
        //        case "JAVASCRIPT": return "JavaScript";
        //        case "PYTHON": return "Python";
        //        case "QUERY LANGUAGE": return "QueryLanguage";
        //        case "TYPESCRIPT": return "TypeScript";
        //        case "VISUAL BASIC": return "VisualBasic";
        //        default: return "CSharp";
        //    }
        //}

        public static List<string> GetTags(this string longText)
        {
            return longText?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList() ?? new List<string>(0);
        }

        public static string GetTemplateFileName(this IProjectTemplate template, string destFolder)
        {
            var destProjectFolder = destFolder.GetFullPath(template.TemplateName);
            return template.IsProject
                ? destProjectFolder.GetFullPath(template.TemplateFileName)
                : destProjectFolder;
        }

        public static string GetProjectName(this string projectTemplateName, string solutionDefaultName)
        {
            var projectName = projectTemplateName;
            if (string.IsNullOrEmpty(projectName))
                return projectName;
            var projectNameEnd = projectTemplateName.StartsWith(solutionDefaultName)
                ? projectTemplateName.Substring(solutionDefaultName.Length)
                : string.Empty;
            if (!string.IsNullOrWhiteSpace(projectNameEnd))
            {
                var pos = projectNameEnd.IndexOf('.');
                if (pos != -1)
                {
                    var mayBeNumber = projectNameEnd.Substring(0, pos);
                    if (int.TryParse(mayBeNumber, out var dummy))
                    {
                        projectNameEnd = projectNameEnd.Substring(pos + 1);
                    }
                }

                projectName = $"$safeprojectname${projectNameEnd}";
            }

            return projectName;
        }

        public static void SetPropertiesFromNodes(this object item, List<XmlNode> nodes)
        {
            var properties = item.GetType().GetProperties();
            foreach (var node in nodes)
            {
                var property = properties.SingleOrDefault(x => x.CanWrite && x.Name.Equals(node.Name, StringComparison.InvariantCultureIgnoreCase));
                if (property == null)
                    continue;

                if (property.PropertyType == typeof(string))
                {
                    property.SetValue(item, node.InnerText.Trim(), null);
                }
                else if (property.PropertyType == typeof(bool))
                {
                    property.SetValue(item, node.InnerText.Trim().ToBool(), null);
                }
                else if (property.PropertyType == typeof(int))
                {
                    property.SetValue(item, node.InnerText.Trim().ToInt(), null);
                }
            }
        }

        public static void SetPropertiesFromNode(this object item, XmlNode node)
        {
            var properties = item.GetType().GetProperties();
            foreach (var attr in node.Attributes?.Cast<XmlAttribute>() ?? new List<XmlAttribute>(0))
            {
                var property = properties.SingleOrDefault(x => x.CanWrite && x.Name.Equals(attr.Name, StringComparison.InvariantCultureIgnoreCase));
                if (property == null)
                    continue;

                if (property.PropertyType == typeof(string))
                {
                    property.SetValue(item, attr.InnerText.Trim(), null);
                }
                else if (property.PropertyType == typeof(bool))
                {
                    property.SetValue(item, attr.InnerText.Trim().ToBool(), null);
                }
                else if (property.PropertyType == typeof(int))
                {
                    property.SetValue(item, attr.InnerText.Trim().ToInt(), null);
                }
            }

            var contentProp = properties.SingleOrDefault(x => x.Name.Equals("Content"));
            contentProp?.SetValue(item, node.InnerText.Trim(), null);
        }

        public static string XmlEncode(this string value)
        {
            return HttpUtility.HtmlEncode(value);
        }

        public static List<string> ValidateInputs(this IProjectTemplate template)
        {
            template.TrimProperties();

            var errors = new List<string>();
            if (string.IsNullOrEmpty(template.TemplateName))
            {
                errors.Add("Missing Template Name");
            }

            if (!template.IsProject)
                return errors;

            if (string.IsNullOrEmpty(template.Description))
            {
                errors.Add("Missing Description");
            }

            if (!template.IconImagePath.FileExists())
            {
                errors.Add("Template Icon doesn't exist");
            }

            if (string.IsNullOrEmpty(template.ProjectType))
            {
                errors.Add("Missing Project Type");
            }

            return errors;
        }

        public static IEnumerable<DirectoryInfo> GetDirectoriesExcept(this DirectoryInfo dirInfo, IEnumerable<string> blackList)
        {
            return dirInfo.GetDirectories().Where(x => !blackList.Contains(x.Name, WildcardComparer))
                .OrderBy(x => x.Name);
        }

        public static IEnumerable<FileInfo> GetFilesExcept(this DirectoryInfo dirInfo, IEnumerable<string> blackList)
        {
            return dirInfo.GetFiles().Where(x => !blackList.Contains(x.Name, WildcardComparer))
                .OrderBy(x => x.Name);
        }

        public static List<IProjectTemplate> GetTemplatesFlattened(this IEnumerable<IProjectTemplate> items)
        {
            var itemsList = items.ToList();
            var itemsCount = itemsList.Count;

            for (var index = 0; index < itemsCount; index++)
            {
                itemsList.AddRange(GetTemplatesFlattened(itemsList[index].Children));
            }

            return itemsList;
        }

        public static string GetTargetTemplatePath(this string targetFolder, string templateName)
        {
            if (string.IsNullOrWhiteSpace(targetFolder) || string.IsNullOrWhiteSpace(templateName))
                return string.Empty;

            return Path.GetFullPath(Path.Combine(targetFolder, templateName.GetSafeFileName() + ".vstemplate"));
        }

        public static bool IsCodeFile(this string extension)
        {
            extension = extension.TrimStart(new[] {'.'}).ToLower();
            switch (extension)
            {
                case "csproj":
                case "cs":
                case "cshtml":
                case "fsproj":
                case "fs":
                case "wapproj":
                case "sqlproj":
                case "vbproj":
                case "vbp":
                case "vcxproj":
                case "njsproj":
                case "pyproj":
                case "razor":
                case "vb":
                case "vbhtml":
                case "xaml":
                    {
                    return true;
                }
            }
            return false;
        }
    }
}
