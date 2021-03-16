using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Extensions.Logging;
using MultiTemplateGenerator.Lib.Models;
using MultiTemplateGenerator.Lib.SolutionParser;

namespace MultiTemplateGenerator.Lib.Generator
{
    public interface ITemplateGeneratorService
    {
        IEnumerable<IProjectTemplate> GetProjectTemplatesFromFolder(string destFolder);
        List<IProjectTemplate> GetProjectTemplates(string solutionFile, IProjectTemplate solutionTemplate, bool copyFromParent);
        IProjectTemplate ReadSolutionTemplate(string solutionTemplateFile);
        void GenerateTemplate(TemplateOptions generateOptions, CancellationToken cancellationToken);
        bool IsTagsSupported { get; }
    }

    public class TemplateGeneratorService : ITemplateGeneratorService
    {
        private readonly ITemplateRepository _templateGenerator;
        private readonly ILogger<TemplateGeneratorService> _logger;
        private readonly string _defaultIconPath;

        public TemplateGeneratorService(ITemplateRepository templateGenerator, ILogger<TemplateGeneratorService> logger)
        {
            _templateGenerator = templateGenerator;
            _logger = logger;
            _defaultIconPath = "Resources\\MultiTemplateGenerator.png".GetAppPath();
        }

        private List<SolutionProjectItem> GetSolutionFileItems(string solutionFile)
        {
            var solution = SolutionFileParser.ParseSolutionFile(solutionFile);
            return solution.GetSortedHierarchy();
        }

        private IProjectTemplate ReadProjectTemplate(string solutionFolder, SolutionProjectItem item, IProjectTemplate parent, bool copyFromParent)
        {
            _logger.LogDebug($"{nameof(ReadProjectTemplate)} solutionFolder: {solutionFolder}");
            return _templateGenerator.ReadProjectTemplate(solutionFolder, item, parent, copyFromParent);
        }

        public List<IProjectTemplate> GetProjectTemplates(string solutionFile, IProjectTemplate solutionTemplate, bool copyFromParent)
        {
            _logger.LogDebug($"{nameof(GetProjectTemplates)} started");
            var solutionFileItems = GetSolutionFileItems(solutionFile);
            var projectItems = new List<IProjectTemplate>();

            foreach (var solutionItem in solutionFileItems)
            {
                projectItems.Add(
                    ReadProjectTemplate(Path.GetDirectoryName(solutionFile), solutionItem, solutionTemplate, copyFromParent));
            }

            _logger.LogDebug($"{nameof(GetProjectTemplates)}: {projectItems.Count} project templates found");
            return projectItems;
        }

        public IProjectTemplate ReadSolutionTemplate(string solutionTemplateFile)
        {
            _logger.LogDebug($"{nameof(ReadSolutionTemplate)} solutionTemplateFile: {solutionTemplateFile}");
            return _templateGenerator.ReadSolutionTemplate(solutionTemplateFile);
        }

        public void GenerateTemplate(TemplateOptions options, CancellationToken ct)
        {
            var projectTemplates = options.ProjectTemplates.ToList();
            _logger.LogDebug($"{nameof(GenerateTemplate)} started: {options} projectTemplates count: {projectTemplates.Count()}");

            var solutionTemplate = options.SolutionTemplate;
            var destFolder = options.TargetFolder;

            //Delete any .zip or .vstemplate files
            var targetDir = new DirectoryInfo(destFolder);
            if (targetDir.Exists)
            {
                var filesToDelete = targetDir.GetFiles("*.zip").ToList();
                filesToDelete.AddRange(targetDir.GetFiles("*.vstemplate", SearchOption.AllDirectories).ToList());
                foreach (var fileToDelete in filesToDelete)
                {
                    ct.ThrowIfCancellationRequested();
                    fileToDelete.Delete();
                }
            }
            else
            {
                targetDir.Create();
            }

            var multiTemplateFile = new FileInfo(options.TargetTemplatePath);

            if (string.IsNullOrWhiteSpace(solutionTemplate.IconImagePath))
            {
                solutionTemplate.IconImagePath = _defaultIconPath;
            }

            solutionTemplate.CopyTemplateIconsTo(destFolder);

            ct.ThrowIfCancellationRequested();

            var projectTemplatesList = projectTemplates.ToList();

            _logger.LogDebug($"Creating {projectTemplatesList.Count} project templates...");
            //Create project templates
            foreach (var projectTemplate in projectTemplatesList)
            {
                ct.ThrowIfCancellationRequested();
                _templateGenerator.CreateProjectTemplate(projectTemplate, options.SolutionFolder, destFolder, true, options.ExcludedFolders, ct);
            }

            ct.ThrowIfCancellationRequested();

            _logger.LogDebug($"Creating solution template...");
            _templateGenerator.CreateSolutionTemplate(multiTemplateFile.FullName, solutionTemplate, projectTemplatesList);

            _logger.LogDebug($"Creating zip file...");
            //Zip files
            var zipFileName = Path.Combine(destFolder, solutionTemplate.TemplateName + ".zip");

            FastZip zip = new FastZip();
            var tempZipFile = Path.GetTempFileName();

            ct.ThrowIfCancellationRequested();
            zip.CreateZip(tempZipFile, destFolder, true, null);

            File.Move(tempZipFile, zipFileName);

            ct.ThrowIfCancellationRequested();
            if (options.AutoImportToVS)
            {
                _logger.LogDebug($"Importing zip file to VS template folder...");
                var vsFolder = Path.Combine(FileExtensions.FindVSTemplateFolder(), solutionTemplate.LanguageTag.GetTemplateFolderNameByLanguage());
                if (!vsFolder.DirectoryExists())
                    vsFolder = FileExtensions.FindVSTemplateFolder();

                File.Copy(zipFileName, Path.Combine(vsFolder, Path.GetFileName(zipFileName)), true);
            }

            _logger.LogInformation("Generating templates completed");
        }

        public bool IsTagsSupported => _templateGenerator.IsTagsSupported;

        public IEnumerable<IProjectTemplate> GetProjectTemplatesFromFolder(string destFolder)
        {
            var destFolderSubDirs = new DirectoryInfo(destFolder).GetDirectories();

            foreach (var subDir in destFolderSubDirs)
            {
                var templateFiles = subDir.GetFiles("*.vstemplate");
                var projectTemplateFile = templateFiles.FirstOrDefault();
                var projectTemplate = projectTemplateFile != null
                    ? _templateGenerator.ReadProjectTemplate(projectTemplateFile.FullName)
                    : _templateGenerator.ReadProjectTemplate(subDir.Name);

                yield return projectTemplate;
            }
        }
    }
}
