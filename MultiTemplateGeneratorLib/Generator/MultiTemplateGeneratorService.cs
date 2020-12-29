using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.Zip;
using MultiTemplateGeneratorLib.Extensions;
using MultiTemplateGeneratorLib.Models;

namespace MultiTemplateGeneratorLib.Generator
{
    public interface IMultiTemplateGeneratorService
    {
        List<SolutionItem> GetSolutionItemsFromFolder(string destFolder);

        int GenerateTemplate(string solutionFile, TemplateOptions options, List<SolutionItem> selectedSolutionItems);

        List<SolutionItem> GetSolutionFileItems(string solutionFile);
    }

    public class MultiTemplateGeneratorService : IMultiTemplateGeneratorService
    {
        private readonly ITemplateGeneratorWriter _templateGenerator;
        private readonly string _defaultIconPath;

        public MultiTemplateGeneratorService(ITemplateGeneratorWriter templateGenerator)
        {
            _templateGenerator = templateGenerator;
            _defaultIconPath = "Resources\\MultiTemplateGenerator.png".GetAppFile();
        }

        public List<SolutionItem> GetSolutionFileItems(string solutionFile)
        {
            List<SolutionItem> solutionItems = new List<SolutionItem>(0);
            if (!string.IsNullOrWhiteSpace(solutionFile))
            {
                if (!File.Exists(solutionFile))
                {
                    throw new FileNotFoundException("Solution file doesn't exist.", solutionFile);
                }

                var solution = SolutionFileParser.ParseSolutionFile(solutionFile);
                solutionItems = solution.GetSortedHierarchy();
            }

            return solutionItems;
        }


        public int GenerateTemplate(string solutionFile, TemplateOptions options, List<SolutionItem> selectedSolutionItems)
        {
            var destFolder = options.DestinationFolder;

            var solutionTemplateFile = new FileInfo(Path.Combine(destFolder, $"{options.DefaultTemplateFileName}"));
            if (solutionTemplateFile.Exists)
            {
                if (MessageBox.Show($"Are you sure you want to overwrite {solutionTemplateFile.FullName}?", @"Overwrite Template", MessageBoxButtons.YesNo) != DialogResult.Yes)
                {
                    return 0;
                }

                solutionTemplateFile.Delete();
            }

            if (string.IsNullOrWhiteSpace(options.Icon))
            {
                options.Icon = _defaultIconPath;
            }
            if (string.IsNullOrWhiteSpace(options.PreviewImage))
            {
                options.PreviewImage = _defaultIconPath;
            }

            options.CopyTemplateIconsTo(destFolder);

            if (options.UseSolution)
            {
                var solutionFolder = Path.GetDirectoryName(solutionFile);
                //Create project templates
                foreach (var solutionItem in selectedSolutionItems)
                {
                    _templateGenerator.CreateProjectTemplate(solutionItem, solutionFolder, destFolder, options, true);
                }
            }

            _templateGenerator.CreateSolutionTemplate(solutionTemplateFile.FullName, options, selectedSolutionItems);

            var zipFilesInFolder = new DirectoryInfo(destFolder).GetFiles("*.zip").ToList();
            foreach (var zipFileInfo in zipFilesInFolder)
            {
                zipFileInfo.Delete();
            }

            //Zip files
            var zipFileName = Path.Combine(destFolder, options.Name + ".zip");
            //if (File.Exists(zipFileName))
            //{
            //    File.Delete(zipFileName);
            //}

            FastZip zip = new FastZip();
            var tempZipFile = Path.GetTempFileName();

            zip.CreateZip(tempZipFile, destFolder, true, null);

            File.Move(tempZipFile, zipFileName);

            if (options.Import)
            {
                var vsFolder = FileExtensions.FindVSTemplateFolder();

                File.Copy(zipFileName, Path.Combine(vsFolder, Path.GetFileName(zipFileName)), true);
            }

            return selectedSolutionItems.Count;
        }

        public List<SolutionItem> GetSolutionItemsFromFolder(string destFolder)
        {
            var destFolderSubDirs = new DirectoryInfo(destFolder).GetDirectories();
            List<SolutionItem> solutionItems = new List<SolutionItem>(destFolderSubDirs.Length);

            foreach (var subDir in destFolderSubDirs)
            {
                var templateFiles = subDir.GetFiles("*.vstemplate");
                var projectTemplate = templateFiles.FirstOrDefault();
                var solutionItem = projectTemplate != null
                    ? new SolutionItem(projectTemplate.Directory?.Name, projectTemplate.FullName.Substring(destFolder.Length + 1))
                    : new SolutionItem(subDir.Name, null);

                solutionItems.Add(solutionItem);
            }

            return solutionItems;
        }
    }
}
