using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using MultiTemplateGenerator.Lib.Generator;
using MultiTemplateGenerator.Lib.Models;

namespace MultiTemplateGenerator.Lib
{
    public static class FileExtensions
    {
        static readonly string AppFile;
        static readonly string AppPath;

        static FileExtensions()
        {
            AppFile = Assembly.GetExecutingAssembly().Location;
            AppPath = Path.GetDirectoryName(AppFile);
        }

        //public static bool IsDirectory(this FileSystemInfo fsi)
        //{
        //    return (fsi.Attributes & FileAttributes.Directory) == FileAttributes.Directory;
        //}

        public static string BrowseForFolder(this string existingPath, bool throwExceptions = true)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if (!string.IsNullOrWhiteSpace(existingPath))
                dlg.SelectedPath = existingPath;

            try
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                    return dlg.SelectedPath;
            }
            catch
            {
                dlg.SelectedPath = null;
                if (dlg.ShowDialog() == DialogResult.OK)
                    return dlg.SelectedPath;
            }

            return null;
        }

        public static void RecreateDirectory(this string path)
        {
            if (Directory.Exists(path))
            {
                try
                {
                    Directory.Delete(path, true);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            Directory.CreateDirectory(path);
        }

        public static void CreateDirectory(this string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        public static string GetAppPath(this string relativePath)
        {
            return AppPath.GetFullPath(relativePath);
        }

        public static string GetFullPath(this string basePath, string relativePath)
        {
            if (string.IsNullOrWhiteSpace(basePath))
            {
                basePath = AppPath;
            }

            return Path.GetFullPath(Path.Combine(basePath, relativePath));
        }

        public static string CopyFileWithExt(this string sourceFile, string destFile)
        {
            Debug.Assert(sourceFile != null, nameof(sourceFile) + " != null");
            Debug.Assert(destFile != null, nameof(destFile) + " != null");

            var ext = Path.GetExtension(sourceFile);
            destFile = Path.ChangeExtension(destFile, ext);

            File.Copy(sourceFile, destFile, true);
            return destFile;
        }

        public static DialogResult ShowDialogPath(this CommonDialog dlg, string path)
        {
            DialogResult dlgResult;
            var fileDialog = dlg as FileDialog;
            var folderDialog = dlg as FolderBrowserDialog;
            try
            {
                if (fileDialog != null)
                {
                    fileDialog.FileName = Path.GetFileName(path);
                    fileDialog.InitialDirectory = Path.GetDirectoryName(path);
                }
                else if (folderDialog != null)
                    folderDialog.SelectedPath = path;
                dlgResult = dlg.ShowDialog();
            }
            catch
            {
                if (fileDialog != null)
                {
                    fileDialog.FileName = string.Empty;
                    fileDialog.InitialDirectory = string.Empty;
                }
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

        public static bool FileExists(this string path)
        {
            try
            {
                return !string.IsNullOrWhiteSpace(path) && File.Exists(path);
            }
            catch
            {
                return false;
            }
        }

        public static bool DirectoryOrFileExists(this string path)
        {
            return DirectoryExists(path) || FileExists(path);
        }


        public static readonly char[] PathSeparators = { '\\', '/' };

        public static string GetDirectoryPath(this string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return filePath;

            filePath = filePath.TrimEnd(PathSeparators);
            var pos = filePath.LastIndexOfAny(PathSeparators);
            return pos != -1 ? filePath.Substring(0, pos) : null;
        }

        public static long GetDirectorySize(this string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath) || !Directory.Exists(fullPath))
                return 0;

            var dirInfo = new DirectoryInfo(fullPath);
            var fsInfoList = dirInfo.GetFiles("*.*", SearchOption.AllDirectories);
            return fsInfoList.Sum(x => x.Length);
        }

        public static void DeleteFile(this string filename, bool throwException = true)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return;

            FileInfo file = new FileInfo(filename);
            if (!file.Exists)
                return;

            if (file.IsReadOnly)
            {
                file.Attributes = FileAttributes.Normal;
            }

            file.Delete();
        }

        public static void DeleteDirectoryContents(this string path)
        {
            foreach (var directory in Directory.GetDirectories(path))
            {
                try
                {
                    Directory.Delete(directory, true);
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e);
                }
            }

            foreach (var filename in Directory.GetFiles(path))
            {
                try
                {
                    filename.DeleteFile();
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e);
                }
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

        public static string ToFileSize(this int size)
        {
            return ToFileSize(Convert.ToUInt64(size));
        }

        public static string ToFileSize(this long size)
        {
            return ToFileSize(Convert.ToUInt64(size));
        }

        public static string ToFileSize(this ulong size)
        {
            if (size < 1024)
                return (size).ToString("F0") + " bytes";
            if (size < Math.Pow(1024, 2))
                return (size / 1024).ToString("F2") + " KB";
            if (size < Math.Pow(1024, 3))
                return (size / Math.Pow(1024, 2)).ToString("F2") + " MB";
            if (size < Math.Pow(1024, 4))
                return (size / Math.Pow(1024, 3)).ToString("F2") + " GB";
            if (size < Math.Pow(1024, 5))
                return (size / Math.Pow(1024, 4)).ToString("F2") + " TB";
            if (size < Math.Pow(1024, 6))
                return (size / Math.Pow(1024, 5)).ToString("F2") + " PB";

            return (size / Math.Pow(1024, 6)).ToString("F2") + " EB";
        }

        public static string FindVSTemplateFolder()
        {
            var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var vsStart = Path.Combine(docs, "Visual Studio ");
            var vsEnd = @"\Templates\ProjectTemplates";

            var directories = Directory.GetDirectories(docs, @"Visual Studio *.*")
                //.Where(x=> x.EndsWith(@"\Templates\ProjectTemplates"))
                .OrderByDescending(x=>x)
                .ToList();

            var templateDirs = new List<string>(directories.Count);
            foreach (var directory in directories)
            {
                if (@$"{directory}{vsEnd}".DirectoryExists())
                    templateDirs.Add(@$"{directory}{vsEnd}");
            }
            
            return templateDirs.FirstOrDefault()
                   ?? throw new DirectoryNotFoundException("Cannot find Visual Studio Template directory.");
        }

        public static void CopyTemplateIconsTo(this IProjectTemplate template, string destFolder)
        {
            //Copy icons
            if (!string.IsNullOrEmpty(template.IconImagePath))
                template.IconImagePath.CopyFileWithExt(Path.Combine(destFolder, "__TemplateIcon.ico"));

            if (!string.IsNullOrEmpty(template.PreviewImagePath))
                template.PreviewImagePath.CopyFileWithExt(Path.Combine(destFolder, "__PreviewImage.ico"));
        }

        public static string ReadFileContent(this string filename)
        {
            using var inStream = new FileStream(filename, FileMode.Open,
                FileAccess.Read, FileShare.ReadWrite);

            using StreamReader sr = new StreamReader(inStream);
            var content = sr.ReadToEnd();
            sr.Close();
            sr.Dispose();
            return content;
        }

        public static void WriteFileContent(this string filename, string content)
        {
            //Ensure the directory exists
            filename.GetDirectoryPath().CreateDirectory();

            using var outStream = new FileStream(filename, FileMode.Create,
                FileAccess.Write, FileShare.ReadWrite);
            using StreamWriter sw = new StreamWriter(outStream);
            sw.Write(content);
            sw.Close();
            sw.Dispose();
        }
    }
}
