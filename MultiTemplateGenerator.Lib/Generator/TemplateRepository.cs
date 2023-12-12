using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using MultiTemplateGenerator.Lib.Helpers;
using MultiTemplateGenerator.Lib.Models;
using MultiTemplateGenerator.Lib.SolutionParser;

namespace MultiTemplateGenerator.Lib.Generator
{
    public interface ITemplateRepository
    {
        void CreateSolutionTemplate(string solutionTemplateFile, IProjectTemplate solutionTemplate, IEnumerable<IProjectTemplate> solutionItems);
        void CreateProjectTemplate(IProjectTemplate template, string mainProjectName, string solutionFolder, string destFolder, bool copyFiles, string excludedFolders, string excludedFiles, CancellationToken ct);
        IProjectTemplate ReadProjectTemplate(string templateFilePath);
        IProjectTemplate ReadProjectTemplate(string solutionFolder, SolutionProjectItem item, IProjectTemplate parent, bool copyFromParent);
        IProjectTemplate ReadSolutionTemplate(string templateFileName);
        bool IsTagsSupported { get; }
        int ReplaceWithTemplateParameters(string fileName, string defaultName, string destFileName);
    }

    public class TemplateRepository : ITemplateRepository
    {
        public TemplateRepository()
        {
            try
            {
                RunningVSVersion = VSHelper.GetCurrentVSVersion();
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
            }
        }

        public string VSTemplateVersion { get; set; } = "2.0.0";

        public Version RunningVSVersion { get; set; } = new Version(16, 8, 0);

        private readonly Version _tagsSupportMinVersion = new Version(16, 1, 2);

        public bool IsTagsSupported => RunningVSVersion >= _tagsSupportMinVersion;

        public void CreateSolutionTemplate(string solutionTemplateFile, IProjectTemplate solutionTemplate, IEnumerable<IProjectTemplate> projectTemplates)
        {
            using var sw = new StreamWriter(File.Open(solutionTemplateFile, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite));

            sw.WriteLine($"<VSTemplate Version=\"{VSTemplateVersion}\" xmlns=\"http://schemas.microsoft.com/developer/vstemplate/2005\" Type=\"ProjectGroup\">");
            sw.WriteLine("  <TemplateData>");
            WriteTemplateData(sw, solutionTemplate);
            sw.WriteLine("  </TemplateData>");
            sw.WriteLine("  <TemplateContent>");
            sw.WriteLine("    <ProjectCollection>");

            var defaultSolutionName = string.IsNullOrWhiteSpace(solutionTemplate.DefaultName)
                ? solutionTemplate.TemplateName
                : solutionTemplate.DefaultName;
            foreach (var projectTemplate in projectTemplates)
            {
                WriteProjectTemplateEntry(sw, projectTemplate, defaultSolutionName, 6);
            }

            sw.WriteLine("    </ProjectCollection>");
            sw.WriteLine("  </TemplateContent>");
            sw.WriteLine("</VSTemplate>");
        }

        public void CreateProjectTemplate(IProjectTemplate template, string mainProjectName, string solutionFolder, string destFolder, bool copyFiles, string excludedFolders, string excludedFiles, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            if (template.IsProject)
            {
                var projectTemplateFile = new FileInfo(template.GetTemplateFileName(destFolder));
                if (!projectTemplateFile.Directory.Exists)
                {
                    projectTemplateFile.Directory.Create();
                }

                //Copy icons
                template.CopyTemplateIconsTo(projectTemplateFile.DirectoryName);

                ct.ThrowIfCancellationRequested();

                var projectFile = new FileInfo(Path.Combine(solutionFolder, template.ProjectFileName));
                var projectFolder = projectFile.Directory.FullName;

                using var sw = new StreamWriter(projectTemplateFile.Create());

                sw.WriteLine($"<VSTemplate Version=\"{VSTemplateVersion}\" xmlns=\"http://schemas.microsoft.com/developer/vstemplate/2005\" Type=\"Project\">");
                sw.WriteLine("  <TemplateData>");
                WriteTemplateData(sw, template);
                sw.WriteLine("  </TemplateData>");
                sw.WriteLine("  <TemplateContent>");
                sw.WriteLine($"    <Project TargetFileName=\"{projectFile.Name}\" File=\"{projectFile.Name}\" ReplaceParameters=\"true\">");

                var blackList = excludedFolders.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                blackList.AddRange(excludedFiles.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
                blackList.AddRange(new List<string> { projectTemplateFile.Name, template.TemplateName + ".zip", "__TemplateIcon.*", "__PreviewImage.*" });

                ct.ThrowIfCancellationRequested();
                WriteFileSystemInfo(sw, projectFile.Directory.FullName, 6, blackList, ct);

                if (copyFiles)
                {
                    var copyOptions = new TemplateFileCopyOptions {BlackList = blackList, DefaultName = mainProjectName };

                    CopyDirectory(projectFolder, Path.Combine(destFolder, template.TemplateName), copyOptions, ct);
                }

                sw.WriteLine("    </Project>");
                sw.WriteLine("  </TemplateContent>");
                sw.WriteLine("</VSTemplate>");
            }

            foreach (var child in template.Children)
            {
                CreateProjectTemplate(child, mainProjectName, solutionFolder, destFolder, copyFiles, excludedFolders, excludedFiles, ct);
            }
        }

        private void WriteFileSystemInfo(StreamWriter sw, string sourceDir, int indent, List<string> blackList, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var padStart = "".PadLeft(indent, ' ');
            var dirInfo = new DirectoryInfo(sourceDir);

            foreach (var subDir in dirInfo.GetDirectoriesExcept(blackList))
            {
                sw.WriteLine($"{padStart}<Folder Name=\"{subDir.Name}\" TargetFolderName=\"{subDir.Name}\">");
                WriteFileSystemInfo(sw, subDir.FullName, indent + 2, blackList, ct);
                sw.WriteLine($"{padStart}</Folder>");
            }

            foreach (var file in dirInfo.GetFilesExcept(blackList))
            {
                sw.WriteLine($"{padStart}<ProjectItem ReplaceParameters=\"true\" TargetFileName=\"{file.Name}\">{file.Name}</ProjectItem>");
            }
        }

        public void WriteXmlTemplateItem(StreamWriter sw, int indent, IEnumerable<XmlTemplateItemBase> templateItems, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var padStart = "".PadLeft(indent, ' ');
            foreach (XmlProjectFolder folder in templateItems.Where(x => x.GetType() == typeof(XmlProjectFolder)).Cast<XmlProjectFolder>())
            {
                sw.WriteLine(padStart + folder.XmlStartTag());
                WriteXmlTemplateItem(sw, indent + 2, folder.Children, ct);
                sw.WriteLine(padStart + folder.XmlEndTag());
            }

            foreach (XmlProjectItem item in templateItems.Where(x => x.GetType() == typeof(XmlProjectItem)).Cast<XmlProjectItem>())
            {
                sw.WriteLine(padStart + item.XmlTag());
            }
        }

        private void WriteTemplateData(StreamWriter sw, IProjectTemplate template)
        {
            var iconExt = Path.GetExtension(template.IconImagePath);
            var previewExt = Path.GetExtension(template.PreviewImagePath);

            sw.WriteLine($"    <Name>{template.TemplateName.XmlEncode()}</Name>");
            sw.WriteLine($"    <Description>{template.Description.XmlEncode()}</Description>");
            sw.WriteLine($"    <DefaultName>{template.DefaultName.XmlEncode()}</DefaultName>");
            sw.WriteLine($"    <ProjectType>{template.ProjectType ?? "CSharp"}</ProjectType>");
            if (!string.IsNullOrEmpty(template.ProjectSubType))
                sw.WriteLine($"    <ProjectSubType>{template.ProjectSubType.XmlEncode()}</ProjectSubType>");

            if (IsTagsSupported)
            {
                /*
                 * Starting in Visual Studio 2019 version 16.1 Preview 2, you can add language, platform, and project type tags to your project templates.
                 */
                if (!string.IsNullOrEmpty(template.LanguageTag) && !template.LanguageTag.Equals("None"))
                    sw.WriteLine($"    <LanguageTag>{template.LanguageTag}</LanguageTag>");
                foreach (var platformTag in template.PlatformTags.GetTags())
                {
                    sw.WriteLine($"    <PlatformTag>{platformTag.XmlEncode()}</PlatformTag>");
                }

                foreach (var projectTypeTag in template.ProjectTypeTags.GetTags())
                {
                    sw.WriteLine($"    <ProjectTypeTag>{projectTypeTag.XmlEncode()}</ProjectTypeTag>");
                }
            }

            sw.WriteLine($"    <CreateNewFolder>{template.CreateNewFolder.ToString().ToLower()}</CreateNewFolder>");
            sw.WriteLine($"    <LocationField>{template.LocationField}</LocationField>");
            sw.WriteLine($"    <EnableLocationBrowseButton>{template.EnableLocationBrowseButton.ToString().ToLower()}</EnableLocationBrowseButton>");
            sw.WriteLine($"    <SortOrder>{template.SortOrder}</SortOrder>");
            sw.WriteLine($"    <CreateInPlace>{template.CreateInPlace.ToString().ToLower()}</CreateInPlace>");
            sw.WriteLine($"    <ProvideDefaultName>{template.ProvideDefaultName.ToString().ToLower()}</ProvideDefaultName>");
            if (!string.IsNullOrEmpty(iconExt))
                sw.WriteLine($"    <Icon>__TemplateIcon{iconExt}</Icon>");
            if (!string.IsNullOrEmpty(previewExt))
                sw.WriteLine($"    <PreviewImage>__PreviewImage{previewExt}</PreviewImage>");
            sw.WriteLine($"    <Hidden>{template.IsHidden.ToString().ToLower()}</Hidden>");

            if (!string.IsNullOrEmpty(template.MaxFrameworkVersion))
                sw.WriteLine($"    <MaxFrameworkVersion>{template.MaxFrameworkVersion}</MaxFrameworkVersion>");
            if (!string.IsNullOrEmpty(template.RequiredFrameworkVersion))
                sw.WriteLine($"    <RequiredFrameworkVersion>{template.RequiredFrameworkVersion}</RequiredFrameworkVersion>");
            if (!string.IsNullOrEmpty(template.FrameworkVersion))
                sw.WriteLine($"    <FrameworkVersion>{template.FrameworkVersion}</FrameworkVersion>");
        }

        private void WriteProjectTemplateEntry(StreamWriter sw, IProjectTemplate template, string solutionDefaultName, int indent)
        {
            var padStart = "".PadLeft(indent, ' ');
            if (template.IsProject)
            {
                var projectName = template.TemplateName.GetProjectName(solutionDefaultName);

                var templateFileName = $"{template.TemplateName.GetSafePathName()}\\{template.TemplateFileName}";
                sw.WriteLine($"{padStart}<ProjectTemplateLink ProjectName=\"{projectName}\" CopyParameters=\"true\">");
                sw.WriteLine($"{padStart}  {templateFileName}");
                sw.WriteLine($"{padStart}</ProjectTemplateLink>");
            }
            else
            {
                sw.WriteLine($"{padStart}<SolutionFolder Name=\"{template.TemplateName}\">");
                foreach (var child in template.Children)
                {
                    WriteProjectTemplateEntry(sw, child, solutionDefaultName, indent + 2);
                }
                sw.WriteLine($"{padStart}</SolutionFolder>");
            }
        }

        public IEnumerable<XmlTemplateItemBase> GetProjectTemplateItems(IEnumerable<XmlNode> projectNodes)
        {
            foreach (var node in projectNodes)
            {
                if (node.Name.Equals("Folder"))
                {
                    XmlProjectFolder item = new XmlProjectFolder
                    {
                        Name = node.Attributes["Name"].InnerText,
                        TargetFolderName = node.Attributes["TargetFolderName"].InnerText
                    };

                    item.Children = GetProjectTemplateItems(node.ChildNodes.Cast<XmlNode>()).ToList();

                    yield return item;
                }
                else if (node.Name.Equals("ProjectItem"))
                {
                    XmlProjectItem item = new XmlProjectItem
                    {
                        Content = node.InnerText.Trim(),
                        TargetFileName = node.Attributes["TargetFileName"]?.InnerText,
                        ReplaceParameters = node.Attributes["ReplaceParameters"]?.InnerText,
                        OpenInEditor = node.Attributes["OpenInEditor"]?.InnerText,
                        OpenInHelpBrowser = node.Attributes["OpenInHelpBrowser"]?.InnerText,
                        OpenInWebBrowser = node.Attributes["OpenInWebBrowser"]?.InnerText,
                        OpenOrder = node.Attributes["OpenOrder"]?.InnerText
                    };

                    yield return item;
                }
                else
                {
                    throw new Exception("Invalid Template node: " + node.Name);
                }
            }
        }

        public IEnumerable<XmlTemplateItemBase> GetTemplateProjectEntries(IEnumerable<XmlNode> projectNodes, List<XmlTemplateItemBase> flattened)
        {
            foreach (var node in projectNodes)
            {
                if (node.Name.Equals("SolutionFolder"))
                {
                    var item = new XmlSolutionFolder();
                    item.SetPropertiesFromNode(node);
                    item.Children = GetTemplateProjectEntries(node.ChildNodes.Cast<XmlNode>(), flattened).ToList();
                    flattened.Add(item);
                    yield return item;
                }
                else if (node.Name.Equals("ProjectTemplateLink"))
                {
                    var item = new XmlProjectTemplateLink();
                    item.SetPropertiesFromNode(node);
                    flattened.Add(item);
                    yield return item;
                }
                else
                {
                    throw new Exception("Invalid Template node: " + node.Name);
                }
            }
        }

        //public IEnumerable<XmlTemplateItemBase> GetProjectEntries(string projectFile)
        //{
        //    XmlDocument xmlDoc = new XmlDocument();
        //    using (var fs = File.OpenRead(projectFile))
        //    {
        //        xmlDoc.Load(fs);

        //        var projectNode = xmlDoc.SelectSingleNode("Project");

        //        if (projectNode == null)
        //        {
        //            throw new ArgumentNullException(nameof(projectNode), @"Cannot find Project node.");
        //        }

        //        var sdk = projectNode.Attributes["Sdk"];
        //        var isSdk = sdk != null;
        //        if (isSdk)
        //        {

        //        }

        //        var itemGroupNodes = xmlDoc.DocumentElement.GetElementsByTagName("ItemGroup").Cast<XmlNode>().ToList();
        //        if (itemGroupNodes.Count == 0 && !isSdk)
        //        {
        //            throw new InvalidDataException($"Template file does not contain ItemGroup tag: {projectFile}.");
        //        }


        //        return GetProjectEntries(itemGroupNodes.SelectMany(x => x.ChildNodes.Cast<XmlNode>()));
        //    }
        //}

        //public IEnumerable<XmlTemplateItemBase> GetProjectEntries(IEnumerable<XmlNode> nodes)
        //{
        //    var excludedNames = new List<string> { "Reference" };
        //    var list = nodes.Where(x => !excludedNames.Contains(x.Name));
        //    foreach (var node in list)
        //    {
        //        yield return new XmlProjectItem
        //        {
        //            Content = node.InnerText
        //        };
        //    }
        //}

        public IProjectTemplate ReadProjectTemplate(string solutionFolder, SolutionProjectItem item, IProjectTemplate parent, bool copyFromParent)
        {
            var template = new ProjectTemplate(item.IsProject);

            template.TemplateName = item.Name;
            template.DefaultName = template.TemplateName;
            template.ProjectFileName = item.ProjectFileName;

            if (copyFromParent)
            {
                template.Description = parent.Description;
                template.IconImagePath = parent.IconImagePath;
                template.PreviewImagePath = parent.PreviewImagePath;
                template.SortOrder = parent.SortOrder;

                template.MaxFrameworkVersion = parent.MaxFrameworkVersion;
                template.RequiredFrameworkVersion = parent.RequiredFrameworkVersion;
                template.FrameworkVersion = parent.FrameworkVersion;
            }

            foreach (var itemChild in item.Children)
            {
                template.Children.Add(ReadProjectTemplate(solutionFolder, itemChild, template, copyFromParent));
            }

            return template;
        }

        public IProjectTemplate ReadProjectTemplate(string templateFilePath)
        {
            var templateFile = new FileInfo(templateFilePath);
            IProjectTemplate template = new ProjectTemplate(templateFile.Exists);
            if (!templateFile.Exists)
            {
                template.TemplateName = templateFile.Name;
                return template;
            }

            XmlDocument xmlDoc = new XmlDocument();
            using var fs = File.OpenRead(templateFilePath);

            xmlDoc.Load(fs);

            ReadTemplateData(xmlDoc, template, templateFile.FullName);

            var templateContentNodes = xmlDoc.DocumentElement.GetElementsByTagName("TemplateContent");
            if (templateContentNodes.Count == 0)
            {
                throw new InvalidDataException($"Template file does not contain TemplateContent tag: {templateFilePath}.");
            }

            var nodes = templateContentNodes.Item(0).ChildNodes.Cast<XmlNode>().ToList();
            var projectNode = nodes.FirstOrDefault(x => x.Name.Equals("Project"));
            template.ProjectFileName = templateFile.Directory.Name + "\\" + projectNode.Attributes["File"].InnerText;

            template.TemplateFileName = templateFile.Name;
            if (string.IsNullOrEmpty(template.LanguageTag))
                template.LanguageTag = template.ProjectFileName.GetLanguageTagFromExtension();

            return template;
        }

        public static void ReadTemplateData(XmlDocument xmlDoc, IProjectTemplate template, string templateFilePath)
        {
            var templateDataNodes = xmlDoc.DocumentElement.GetElementsByTagName("TemplateData");
            if (templateDataNodes.Count == 0)
            {
                throw new InvalidDataException($"Template file does not contain TemplateData tag: {templateFilePath}.");
            }

            var nodes = templateDataNodes.Item(0).ChildNodes.Cast<XmlNode>().ToList();
            if (nodes.Count == 0)
            {
                throw new InvalidDataException($"Template file does not contain TemplateData nodes: {templateFilePath}.");
            }

            template.SetPropertiesFromNodes(nodes);

            template.TemplateName = nodes.FirstOrDefault(x => x.Name.Equals("Name"))?.InnerText;

            var iconPath = nodes.FirstOrDefault(x => x.Name.Equals("Icon"))?.InnerText;
            if (!string.IsNullOrEmpty(iconPath))
                template.IconImagePath = Path.Combine(Path.GetDirectoryName(templateFilePath), iconPath);
            iconPath = nodes.FirstOrDefault(x => x.Name.Equals("PreviewImage"))?.InnerText;
            if (!string.IsNullOrEmpty(iconPath))
                template.PreviewImagePath = Path.Combine(Path.GetDirectoryName(templateFilePath), iconPath);
            template.IsHidden = nodes.FirstOrDefault(x => x.Name.Equals("Hidden"))?.InnerText.ToBool() ?? false;

            if (!Enum.TryParse(nodes.FirstOrDefault(x => x.Name.Equals("LocationField"))?.InnerText, true, out LocationFieldType locationField))
                locationField = LocationFieldType.Enabled;
            template.LocationField = locationField;

            //Tags
            var platformTags = nodes.Where(x => x.Name.Equals("PlatformTag")).Select(x => x.InnerText).ToList();
            template.PlatformTags = string.Join(",", platformTags);
            var projectTypeTags = nodes.Where(x => x.Name.Equals("ProjectTypeTag")).Select(x => x.InnerText).ToList();
            template.ProjectTypeTags = string.Join(",", projectTypeTags);
        }

        public IProjectTemplate ReadSolutionTemplate(string templateFileName)
        {
            var solutionFolder = Path.GetDirectoryName(templateFileName);

            var solutionTemplate = new ProjectTemplate(true);

            XmlDocument xmlDoc = new XmlDocument();
            using var fs = File.OpenRead(templateFileName);

            xmlDoc.Load(fs);

            ReadTemplateData(xmlDoc, solutionTemplate, templateFileName);

            var templateContentNodes = xmlDoc.DocumentElement.GetElementsByTagName("TemplateContent");
            if (templateContentNodes.Count == 0)
            {
                throw new InvalidDataException($"Template file does not contain TemplateContent tag: {templateFileName}.");
            }

            var nodes = templateContentNodes.Item(0).ChildNodes.Cast<XmlNode>().ToList();
            var projectNode = nodes.FirstOrDefault(x => x.Name.Equals("ProjectCollection"));
            if (projectNode == null)
            {
                throw new InvalidDataException($"Template file does not contain ProjectCollection tag: {templateFileName}.");
            }

            var flattenedList = new List<XmlTemplateItemBase>();
            List<XmlTemplateItemBase> allTemplateItems = GetTemplateProjectEntries(projectNode.ChildNodes.Cast<XmlNode>(), flattenedList).ToList();

            ////Read all vstemplate files
            //var projectTemplateFiles = flattenedList.GetProjectTemplateLinks()
            //    .Select(x => Path.Combine(solutionFolder, x.Content))
            //    .Where(x=>x.FileExists()).ToList();

            //var projectTemplates = new List<IProjectTemplate>();
            //foreach (var projectTemplateFile in projectTemplateFiles)
            //{
            //    projectTemplates.Add(ReadProjectTemplate(projectTemplateFile));
            //}

            var firstProjectName = allTemplateItems.GetProjectTemplateLinks().FirstOrDefault()?.ProjectName;

            var orderedChildItems = ToProjectTemplates(allTemplateItems, solutionFolder).ToList();
            var mainProject = orderedChildItems.GetTemplatesFlattened().Single(x => x.TemplateName.Equals(firstProjectName));
            mainProject.IsMainProject = true;

            solutionTemplate.Children.AddRange(orderedChildItems);
            return solutionTemplate;
        }

        public IEnumerable<IProjectTemplate> ToProjectTemplates(IEnumerable<XmlTemplateItemBase> xmlItems, string solutionFolder)
        {
            var projectItems = new List<IProjectTemplate>();
            foreach (var xmlSolutionFolder in xmlItems.GetSolutionFolders())
            {
                var solutionFolderTemplate = ReadProjectTemplate(xmlSolutionFolder.Name);
                projectItems.Add(solutionFolderTemplate);
                solutionFolderTemplate.Children.AddRange(ToProjectTemplates(xmlSolutionFolder.Children, solutionFolder));
            }

            foreach (var xmlProjectLink in xmlItems.GetProjectTemplateLinks())
            {
                var projectTemplateFile = Path.Combine(solutionFolder, xmlProjectLink.Content);
                if (!projectTemplateFile.FileExists())
                    continue;
                projectItems.Add(ReadProjectTemplate(projectTemplateFile));
            }

            return projectItems;
        }

        public IEnumerable<XmlTemplateItemBase> GetTemplateProjectEntries(IEnumerable<XmlNode> projectNodes)
        {
            foreach (var node in projectNodes)
            {
                if (node.Name.Equals("Folder"))
                {
                    XmlProjectFolder item = new XmlProjectFolder
                    {
                        Name = node.Attributes["Name"].InnerText,
                        TargetFolderName = node.Attributes["TargetFolderName"].InnerText
                    };

                    item.Children = GetTemplateProjectEntries(node.ChildNodes.Cast<XmlNode>()).ToList();

                    yield return item;
                }
                else if (node.Name.Equals("ProjectItem"))
                {
                    XmlProjectItem item = new XmlProjectItem
                    {
                        Content = node.InnerText,
                        TargetFileName = node.Attributes["TargetFileName"]?.InnerText,
                        ReplaceParameters = node.Attributes["ReplaceParameters"]?.InnerText,
                        OpenInEditor = node.Attributes["OpenInEditor"]?.InnerText,
                        OpenInHelpBrowser = node.Attributes["OpenInHelpBrowser"]?.InnerText,
                        OpenInWebBrowser = node.Attributes["OpenInWebBrowser"]?.InnerText,
                        OpenOrder = node.Attributes["OpenOrder"]?.InnerText
                    };

                    yield return item;
                }
                else
                {
                    throw new Exception("Invalid Template node: " + node.Name);
                }
            }
        }

        //public IEnumerable<XmlTemplateItemBase> GetProjectEntries(string projectFile)
        //{
        //    XmlDocument xmlDoc = new XmlDocument();
        //    using (var fs = File.OpenRead(projectFile))
        //    {
        //        xmlDoc.Load(fs);

        //        var projectNode = xmlDoc.SelectSingleNode("Project");

        //        if (projectNode == null)
        //        {
        //            throw new ArgumentNullException(nameof(projectNode), @"Cannot find Project node.");
        //        }

        //        var sdk = projectNode.Attributes["Sdk"];
        //        var isSdk = sdk != null;
        //        if (isSdk)
        //        {

        //        }

        //        var itemGroupNodes = xmlDoc.DocumentElement.GetElementsByTagName("ItemGroup").Cast<XmlNode>().ToList();
        //        if (itemGroupNodes.Count == 0 && !isSdk)
        //        {
        //            throw new InvalidDataException($"Template file does not contain ItemGroup tag: {projectFile}.");
        //        }

        //        return GetProjectEntries(itemGroupNodes.SelectMany(x => x.ChildNodes.Cast<XmlNode>()));
        //    }
        //}

        //public IEnumerable<XmlTemplateItemBase> GetProjectEntries(IEnumerable<XmlNode> nodes)
        //{
        //    var excludedNames = new List<string> { "Reference" };
        //    var list = nodes.Where(x => !excludedNames.Contains(x.Name));
        //    foreach (var node in list)
        //    {
        //        yield return new XmlProjectItem
        //        {
        //            Content = node.InnerText
        //        };
        //    }
        //}

        public void CopyDirectory(string sourceFolder, string destFolder, TemplateFileCopyOptions copyOptions, System.Threading.CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            var dir = new DirectoryInfo(sourceFolder);
            var destDir = new DirectoryInfo(destFolder);
            var files = dir.GetFilesExcept(copyOptions.BlackList).ToList();
            if (!destDir.Exists && files.Count != 0)
            {
                destDir.Create();
            }

            foreach (FileInfo file in files)
            {
                ct.ThrowIfCancellationRequested();

                if (file.Extension.IsCodeFile())
                {
                    ReplaceWithTemplateParameters(file.FullName, copyOptions.DefaultName, Path.Combine(destFolder, file.Name));
                }
                else
                {
                    file.CopyTo(Path.Combine(destFolder, file.Name), true);
                }
            }

            foreach (DirectoryInfo subDir in dir.GetDirectoriesExcept(copyOptions.BlackList))
            {
                CopyDirectory(subDir.FullName, Path.Combine(destFolder, subDir.Name), copyOptions, ct);
            }
        }


        public int ReplaceWithTemplateParameters(string fileName, string defaultName, string destFileName)
        {
            var allLines = File.ReadLines(fileName).ToList();
            var newLines = new List<string>(allLines.Count);
            var changesMade = 0;
            foreach (var line in allLines)
            {
                var newLine = line
                    .Replace($"{defaultName}", "$ext_safeprojectname$");
                newLines.Add(newLine);

                if (!line.Equals(newLine))
                {
                    changesMade++;
                }
            }

            if (changesMade != 0)
            {
                if (File.Exists(destFileName))
                {
                    File.Delete(destFileName);
                }
                File.WriteAllLines(destFileName, newLines);
            }
            else
            {
                File.Copy(fileName, destFileName, true);
            }

            return changesMade;
        }
    }
}
