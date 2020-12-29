using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using MultiTemplateGeneratorLib.Extensions;
using MultiTemplateGeneratorLib.Models;

namespace MultiTemplateGeneratorLib.Generator
{
    public interface ITemplateGeneratorWriter
    {
        void CreateSolutionTemplate(string solutionTemplateFile, TemplateOptions options, List<SolutionItem> solutionItems);
        void CreateProjectTemplate(SolutionItem solutionItem, string solutionFolder, string destFolder, TemplateOptions options, bool copyFiles);
    }

    public class TemplateGeneratorWriter : ITemplateGeneratorWriter
    {
        public void CreateSolutionTemplate(string solutionTemplateFile, TemplateOptions options, List<SolutionItem> solutionItems)
        {
            using (var sw = new StreamWriter(File.Open(solutionTemplateFile, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite)))
            {
                sw.WriteLine("<VSTemplate Version=\"3.0.0\" xmlns=\"http://schemas.microsoft.com/developer/vstemplate/2005\" Type=\"ProjectGroup\">");
                sw.WriteLine("  <TemplateData>");
                WriteTemplateDataOptions(sw, options, false);
                sw.WriteLine("  </TemplateData>");
                sw.WriteLine("  <TemplateContent>");
                sw.WriteLine("    <ProjectCollection>");

                foreach (var solutionItem in solutionItems)
                {
                    WriteProjectTemplateEntry(sw, solutionItem, options.DefaultName, 6);
                }

                sw.WriteLine("    </ProjectCollection>");
                sw.WriteLine("  </TemplateContent>");
                sw.WriteLine("</VSTemplate>");
            }
        }

        public void CreateProjectTemplate(SolutionItem solutionItem, string solutionFolder, string destFolder, TemplateOptions options, bool copyFiles)
        {
            if (solutionItem.IsProject)
            {
                var projectTemplateFile = new FileInfo(Path.Combine(destFolder, solutionItem.Name, $"{solutionItem.DefaultTemplateFileName}"));
                if (!projectTemplateFile.Directory.Exists)
                {
                    projectTemplateFile.Directory.Create();
                }

                //Copy icons
                options.CopyTemplateIconsTo(projectTemplateFile.DirectoryName);

                var projectFile = new FileInfo(Path.Combine(solutionFolder, solutionItem.FileName));

                using (var sw = new StreamWriter(projectTemplateFile.Create()))
                {
                    sw.WriteLine("<VSTemplate Version=\"3.0.0\" xmlns=\"http://schemas.microsoft.com/developer/vstemplate/2005\" Type=\"Project\">");
                    sw.WriteLine("  <TemplateData>");
                    WriteTemplateDataOptions(sw, options, true);
                    sw.WriteLine("  </TemplateData>");
                    sw.WriteLine("  <TemplateContent>");
                    sw.WriteLine($"    <Project TargetFileName=\"{projectFile.Name}\" File=\"{projectFile.Name}\" ReplaceParameters=\"true\">");

                    var blackList = new List<string> { "bin", "obj" };

                    var fileSystemsInProject = projectFile.Directory?.GetFileSystemInfos()
                        .Where(x => !blackList.Contains(x.Name))
                        .OrderFileSystemInfos() ?? new List<FileSystemInfo>(0);

                    foreach (var fileSystemInfo in fileSystemsInProject)
                    {
                        WriteFileSystemInfo(sw, fileSystemInfo, 6, blackList);
                    }

                    if (copyFiles)
                    {
                        projectFile.Directory?.FullName.CopyDirectory(Path.Combine(destFolder, solutionItem.Name), blackList);
                    }

                    sw.WriteLine("    </Project>");
                    sw.WriteLine("  </TemplateContent>");
                    sw.WriteLine("</VSTemplate>");
                }
            }

            foreach (var child in solutionItem.Children)
            {
                CreateProjectTemplate(child, solutionFolder, destFolder, options, copyFiles);
            }
        }

        private void WriteFileSystemInfo(StreamWriter sw, FileSystemInfo fsi, int indent, List<string> blackList)
        {
            var padStart = "".PadLeft(indent, ' ');
            if ((fsi.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
            {
                var dir = (DirectoryInfo)fsi;
                if (fsi.Name.Equals("obj", StringComparison.InvariantCultureIgnoreCase) || fsi.Name.Equals("bin", StringComparison.InvariantCultureIgnoreCase))
                    return;

                sw.WriteLine($"{padStart}<Folder Name=\"{fsi.Name}\" TargetFolderName=\"{fsi.Name}\">");
                foreach (var child in dir.GetFileSystemInfos()
                    .Where(x => !blackList.Contains(x.Name))
                    .OrderFileSystemInfos())
                {
                    WriteFileSystemInfo(sw, child, indent + 2, blackList);
                }
                sw.WriteLine($"{padStart}</Folder>");
            }
            else
            {
                sw.WriteLine($"{padStart}<ProjectItem ReplaceParameters=\"true\" TargetFileName=\"{fsi.Name}\">{fsi.Name}</ProjectItem>");
            }
        }

        private void WriteTemplateDataOptions(StreamWriter sw, TemplateOptions options, bool createNewFolder)
        {
            var iconExt = Path.GetExtension(options.Icon);
            var previewExt = Path.GetExtension(options.PreviewImage);

            sw.WriteLine($"    <Name>{options.Name}</Name>");
            sw.WriteLine($"    <Description>{options.Description}</Description>");
            sw.WriteLine($"    <DefaultName>{options.DefaultName}</DefaultName>");
            sw.WriteLine($"    <ProjectType>{options.ProjectType}</ProjectType>");
            sw.WriteLine($"    <ProjectSubType>{options.ProjectSubType}</ProjectSubType>");
            sw.WriteLine($"    <LanguageTag>{options.LanguageTag}</LanguageTag>");
            foreach (var platformTag in options.PlatformTags)
            {
                sw.WriteLine($"    <PlatformTag>{platformTag}</PlatformTag>");
            }

            foreach (var projectTypeTag in options.ProjectTypeTags)
            {
                sw.WriteLine($"    <ProjectTypeTag>{projectTypeTag}</ProjectTypeTag>");
            }
            sw.WriteLine($"    <LocationField>Enabled</LocationField>");
            sw.WriteLine($"    <EnableLocationBrowseButton>true</EnableLocationBrowseButton>");
            sw.WriteLine($"    <SortOrder>1000</SortOrder>");
            sw.WriteLine($"    <CreateInPlace>true</CreateInPlace>");
            sw.WriteLine($"    <ProvideDefaultName>true</ProvideDefaultName>");
            sw.WriteLine($"    <Hidden>{createNewFolder.ToString().ToLower()}</Hidden>");
            sw.WriteLine($"    <Icon>__TemplateIcon{iconExt}</Icon>");
            sw.WriteLine($"    <PreviewImage>__PreviewImage{previewExt}</PreviewImage>");
            sw.WriteLine($"    <Hidden>{options.HiddenProjects.ToString().ToLower()}</Hidden>");
        }

        private void WriteProjectTemplateEntry(StreamWriter sw, SolutionItem solutionItem, string defaultName, int indent)
        {
            var padStart = "".PadLeft(indent, ' ');
            if (solutionItem.IsProject)
            {
                var projectName = solutionItem.Name;
                var projectNameEnd = solutionItem.Name.StartsWith(defaultName)
                    ? solutionItem.Name.Substring(defaultName.Length).Trim('.')
                    : string.Empty;
                if (!string.IsNullOrWhiteSpace(projectNameEnd))
                {
                    var pos = projectNameEnd.IndexOf('.');
                    if (pos != -1)
                    {
                        var mayBeNumber = projectNameEnd.Substring(0, pos);
                        if (int.TryParse(mayBeNumber, out var dummy))
                        {
                            projectNameEnd = projectNameEnd.Substring(pos + 1).Trim('.');
                        }
                    }

                    projectName = $"$safeprojectname$.{projectNameEnd}";
                }

                var templateFileName = solutionItem.TemplateFileName ?? $"{solutionItem.Name}\\{solutionItem.DefaultTemplateFileName}";
                sw.WriteLine($"{padStart}<ProjectTemplateLink ProjectName=\"{projectName}\">");
                sw.WriteLine($"{padStart}  {templateFileName}");
                sw.WriteLine($"{padStart}</ProjectTemplateLink>");
            }
            else
            {
                sw.WriteLine($"{padStart}<SolutionFolder Name=\"{solutionItem.Name}\">");
                foreach (var child in solutionItem.Children)
                {
                    WriteProjectTemplateEntry(sw, child, defaultName, indent + 2);
                }
                sw.WriteLine($"{padStart}</SolutionFolder>");
            }
        }
    }
}
