using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using ICSharpCode.SharpZipLib.Zip;
using MultiTemplateGenerator.Lib.Models;
using MultiTemplateGenerator.Lib.SolutionParser;
using Serilog;

namespace MultiTemplateGenerator.Lib.Generator
{
    public interface ITemplateGeneratorService
    {
        IEnumerable<IProjectTemplate> GetProjectTemplatesFromFolder(string destFolder);
        List<IProjectTemplate> GetProjectTemplates(string solutionFile, IProjectTemplate solutionTemplate);
        IProjectTemplate ReadSolutionTemplate(string solutionTemplateFile);
        void GenerateTemplate(TemplateOptions generateOptions, CancellationToken cancellationToken);
        bool IsTagsSupported { get; }
    }

    public class TemplateGeneratorService : ITemplateGeneratorService
    {
        private readonly ITemplateRepository _templateGenerator;
        private readonly ILogger _logger;
        private readonly string _defaultIconPath;

        public TemplateGeneratorService(ITemplateRepository templateGenerator, ILogger logger)
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

        private IProjectTemplate ReadProjectTemplate(string solutionFolder, SolutionProjectItem item, IProjectTemplate parent)
        {
            _logger.Debug($"{nameof(ReadProjectTemplate)} solutionFolder: {solutionFolder}");
            return _templateGenerator.ReadProjectTemplate(solutionFolder, item, parent);
        }

        public List<IProjectTemplate> GetProjectTemplates(string solutionFile, IProjectTemplate solutionTemplate)
        {
            _logger.Debug($"{nameof(GetProjectTemplates)} started");
            var solutionFileItems = GetSolutionFileItems(solutionFile);
            var projectItems = new List<IProjectTemplate>();

            foreach (var solutionItem in solutionFileItems)
            {
                projectItems.Add(
                    ReadProjectTemplate(Path.GetDirectoryName(solutionFile), solutionItem, solutionTemplate));
            }

            _logger.Debug($"{nameof(GetProjectTemplates)}: {projectItems.Count} project templates found");
            return projectItems;
        }

        public IProjectTemplate ReadSolutionTemplate(string solutionTemplateFile)
        {
            _logger.Debug($"{nameof(ReadSolutionTemplate)} solutionTemplateFile: {solutionTemplateFile}");
            return _templateGenerator.ReadSolutionTemplate(solutionTemplateFile);
        }

        public void GenerateTemplate(TemplateOptions options, CancellationToken ct)
        {
            var projectTemplates = options.ProjectTemplates.ToList();
            _logger.Debug($"{nameof(GenerateTemplate)} started: {options} projectTemplates count: {projectTemplates.Count()}");

            var solutionTemplate = options.SolutionTemplate;
            var destFolder = options.TargetFolder;


            //Delete any .zip or .vstemplate files
            var destDirInfo = new DirectoryInfo(destFolder);
            if (destDirInfo.Exists)
            {
                var filesToDelete = destDirInfo.GetFiles("*.zip").ToList();
                filesToDelete.AddRange(destDirInfo.GetFiles("*.vstemplate", SearchOption.AllDirectories).ToList());
                foreach (var fileToDelete in filesToDelete)
                {
                    ct.ThrowIfCancellationRequested();
                    fileToDelete.Delete();
                }
            }
            else
            {
                destDirInfo.Create();
            }

            var multiTemplateFile = new FileInfo(options.TargetTemplatePath);

            if (string.IsNullOrWhiteSpace(solutionTemplate.IconImagePath))
            {
                solutionTemplate.IconImagePath = _defaultIconPath;
            }

            solutionTemplate.CopyTemplateIconsTo(destFolder);

            ct.ThrowIfCancellationRequested();

            var projectTemplatesList = projectTemplates.ToList();

            if (options.UseSolution)
            {
                _logger.Debug($"Creating {projectTemplatesList.Count} project templates...");
                //Create project templates
                foreach (var projectTemplate in projectTemplatesList)
                {
                    _templateGenerator.CreateProjectTemplate(projectTemplate, options.SolutionFolder, destFolder, true, ct);
                }
            }

            _logger.Debug($"Creating solution template...");
            _templateGenerator.CreateSolutionTemplate(multiTemplateFile.FullName, solutionTemplate, projectTemplatesList);

            _logger.Debug($"Creating zip file...");
            //Zip files
            var zipFileName = Path.Combine(destFolder, solutionTemplate.TemplateName + ".zip");

            FastZip zip = new FastZip();
            var tempZipFile = Path.GetTempFileName();

            zip.CreateZip(tempZipFile, destFolder, true, null);

            File.Move(tempZipFile, zipFileName);

            if (options.AutoImportToVS)
            {
                _logger.Debug($"Importing zip file to VS template folder...");
                var vsFolder = Path.Combine(FileExtensions.FindVSTemplateFolder(), solutionTemplate.LanguageTag.GetTemplateFolderNameByLanguage());
                if (!vsFolder.DirectoryExists())
                    vsFolder = FileExtensions.FindVSTemplateFolder();

                File.Copy(zipFileName, Path.Combine(vsFolder, Path.GetFileName(zipFileName)), true);
            }

            _logger.Information("Generating templates completed");
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
