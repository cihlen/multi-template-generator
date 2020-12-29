using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using MultiTemplateGeneratorLib.Models;

namespace MultiTemplateGeneratorLib.Extensions
{
    public static class FileExtensions
    {
        public static bool IsDirectory(this FileSystemInfo fsi)
        {
            return (fsi.Attributes & FileAttributes.Directory) == FileAttributes.Directory;
        }

        public static IEnumerable<FileSystemInfo> OrderFileSystemInfos(this IEnumerable<FileSystemInfo> fileSystemInfos)
        {
            return fileSystemInfos
                .OrderByDescending(x => x.IsDirectory())
                .ThenBy(x => x.CreationTime);
        }

        static string _appFile;
        static string _appPath;
        public static string GetAppFile(this string relativePath)
        {
            if (string.IsNullOrEmpty(_appFile))
            {
                _appFile = Assembly.GetExecutingAssembly().Location;
                _appPath = Path.GetDirectoryName(_appFile);
            }

            Debug.Assert(_appPath != null, nameof(_appPath) + " != null");
            return Path.GetFullPath(Path.Combine(_appPath, relativePath));
        }

        public static void CopyFileWithExt(this string sourceFile, string destFile, out string ext)
        {
            ext = Path.GetExtension(sourceFile);
            destFile = Path.ChangeExtension(destFile, ext);

            Debug.Assert(sourceFile != null, nameof(sourceFile) + " != null");
            File.Copy(sourceFile, destFile ?? throw new ArgumentNullException(nameof(destFile)), true);
        }

        public static DialogResult ShowDialogPath(this CommonDialog dlg, string path)
        {
            DialogResult dlgResult;
            var fileDialog = dlg as FileDialog;
            var folderDialog = dlg as FolderBrowserDialog;
            try
            {
                if (fileDialog != null)
                    fileDialog.FileName = path;
                else if (folderDialog != null)
                    folderDialog.SelectedPath = path;
                dlgResult = dlg.ShowDialog();
            }
            catch
            {
                if (fileDialog != null)
                    fileDialog.FileName = string.Empty;
                else if (folderDialog != null)
                    folderDialog.SelectedPath = string.Empty;
                dlgResult = dlg.ShowDialog();
            }

            return dlgResult;
        }

        public static bool DirectoryExists(this string path)
        {
            try
            {
                return !string.IsNullOrWhiteSpace(path) && Directory.Exists(path);
            }
            catch
            {
                return false;
            }
        }

        public static void CopyDirectory(this string sourceFolder, string destFolder, List<string> blackList)
        {
            var dir = new DirectoryInfo(sourceFolder);
            var destDir = new DirectoryInfo(destFolder);
            var files = dir.GetFiles().Where(x => !blackList.Contains(x.Name)).ToList();
            if (!destDir.Exists && files.Count != 0)
            {
                destDir.Create();
            }
            foreach (FileInfo file in files)
            {
                file.CopyTo(Path.Combine(destFolder, file.Name), true);
            }

            foreach (DirectoryInfo subDir in dir.GetDirectories().Where(x => !blackList.Contains(x.Name)))
            {
                CopyDirectory(subDir.FullName, Path.Combine(destFolder, subDir.Name), blackList);
            }
        }

        static readonly List<char> InvalidFileNameChars = Path.GetInvalidFileNameChars().ToList();
        static readonly List<char> InvalidPathChars = Path.GetInvalidPathChars().ToList();

        private static List<char> _invalidFullPathChars;
        static List<char> InvalidFullPathChars
        {
            get
            {
                if (_invalidFullPathChars == null)
                {
                    _invalidFullPathChars = new List<char>(InvalidFileNameChars);
                    _invalidFullPathChars.AddRange(InvalidPathChars);
                }

                return _invalidFullPathChars;
            }
        }

        public static string GetSafeFileName(this string text)
        {
            var cleanText = new StringBuilder();

            foreach (char c in text)
            {
                cleanText.Append(!InvalidFileNameChars.Contains(c) ? c : '_');
            }
            return cleanText.ToString();
        }

        public static string GetSafePathName(this string text)
        {
            var cleanText = new StringBuilder();

            foreach (char c in text)
            {
                cleanText.Append(!InvalidPathChars.Contains(c) ? c : '_');
            }
            return cleanText.ToString();
        }

        public static string GetSafeFullPathName(this string text)
        {
            var cleanText = new StringBuilder();

            foreach (char c in text)
            {
                cleanText.Append(!InvalidFullPathChars.Contains(c) ? c : '_');
            }
            return cleanText.ToString();
        }

        public static string FindVSTemplateFolder()
        {
            var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var vsStart = Path.Combine(docs, "Visual Studio ");
            var vsEnd = @"\Templates\ProjectTemplates";

            if ($"{vsStart}2019{vsEnd}".DirectoryExists())
            {
                return $"{vsStart}2019{vsEnd}";
            }

            if ($"{vsStart}2017{vsEnd}".DirectoryExists())
            {
                return $"{vsStart}2017{vsEnd}";
            }

            throw new DirectoryNotFoundException("Cannot find Visual Studio Template directory.");
        }

        public static void CopyTemplateIconsTo(this TemplateOptions options, string destFolder)
        {
            //Copy icons
            try
            {
                options.Icon.CopyFileWithExt(Path.Combine(destFolder, "__TemplateIcon.ico"), out var iconExt);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
            }

            try
            {
                options.PreviewImage.CopyFileWithExt(Path.Combine(destFolder, "__PreviewImage.ico"), out var previewExt);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
            }
        }
    }
}
