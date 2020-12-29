using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using MultiTemplateGeneratorLib.Controls;
using MultiTemplateGeneratorLib.Extensions;
using MultiTemplateGeneratorLib.Generator;
using MultiTemplateGeneratorLib.Models;
using MultiTemplateGeneratorLib.Properties;

namespace MultiTemplateGeneratorLib.Views
{
    public partial class ProjectSelectorForm : Form
    {
        private readonly IMultiTemplateGeneratorService _generatorService;

        public ProjectSelectorForm()
        {
            InitializeComponent();

            chkImport.Checked = Settings.Default.Import;

            cboProjectTypeTag.DisplayMember = cboPlatformTag.DisplayMember = "Name";
            cboProjectTypeTag.ValueSeparator = cboPlatformTag.ValueSeparator = ", ";

            var defaultPlatformTags = new List<string> { "Android", "Azure", "iOS", "Linux", "macOS", "tvOS", "Windows", "Xbox" };

            cboPlatformTag.Items.Clear();
            for (int i = 0; i < defaultPlatformTags.Count; i++)
            {
                var chkItem = new CCBoxItem(defaultPlatformTags[i], i);
                cboPlatformTag.Items.Add(chkItem);
            }

            var defaultProjectTypeTags = new List<string> { "Cloud", "Console", "Desktop", "Extensions", "Games", "IoT", "Library",
                "Machine Learning", "Mobile", "Office", "Other", "Service", "Test", "UWP", "Web" };

            cboProjectTypeTag.Items.Clear();
            for (int i = 0; i < defaultProjectTypeTags.Count; i++)
            {
                var chkItem = new CCBoxItem(defaultProjectTypeTags[i], i);
                cboProjectTypeTag.Items.Add(chkItem);
            }
        }

        public ProjectSelectorForm(IMultiTemplateGeneratorService generatorService, List<SolutionItem> solutionItems, bool useSolution)
        : this()
        {
            _generatorService = generatorService;

            SolutionItems = solutionItems;
            if (SolutionItems.Count == 0)
            {
                useSolution = false;
                rbUseSolution.Enabled = false;
            }

            rbUseSolution.Checked = useSolution;
            rbUseFolder.Checked = !rbUseSolution.Checked;
        }

        public List<SolutionItem> SolutionItems { get; }
        public List<SolutionItem> FolderSolutionItems { get; private set; } = new List<SolutionItem>(0);
        public List<SolutionItem> SelectedSolutionItems { get; } = new List<SolutionItem>(0);

        public string ProjectType { get; set; } = "CSharp";

        public string LanguageTag { get; set; } = "C#";
        public TemplateOptions GetTemplateOptions()
        {
            var platformTags = new List<string>();
            foreach (var item in cboPlatformTag.CheckedItems)
            {
                platformTags.Add(item.ToString().ToLower().Replace(" ", ""));
            }

            platformTags = cboPlatformTag.Text.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.ToLower().Replace(" ", "")).ToList();
            var projectTypeTags = cboProjectTypeTag.Text.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.ToLower().Replace(" ", "")).ToList();
            return new TemplateOptions
            {
                Name = txtTemplateName.Text,
                DefaultName = txtDefaultName.Text,
                Description = txtDescription.Text,
                PlatformTags = platformTags,
                ProjectTypeTags = projectTypeTags,
                ProjectSubType = txtProjectSubType.Text,
                Icon = txtIcon.Text,
                PreviewImage = txtPreviewImage.Text,
                DestinationFolder = txtDestination.Text,
                UseSolution = rbUseSolution.Checked,
                Import = chkImport.Checked
            };
        }

        public void SetTemplateOptions(string defaultName)
        {
            txtTemplateName.Text = defaultName?.Replace('.', ' ') ?? Settings.Default.TemplateName;
            txtDescription.Text = Settings.Default.Description;
            txtDefaultName.Text = defaultName ?? txtTemplateName.Text;
            LanguageTag = Settings.Default.LanguageTag;
            ProjectType = Settings.Default.ProjectType;

            cboPlatformTag.SetItemsChecked(Settings.Default.PlatformTag, ',');
            cboProjectTypeTag.SetItemsChecked(Settings.Default.ProjectTypeTag, ',');

            txtProjectSubType.Text = Settings.Default.ProjectSubType;

            txtIcon.Text = Settings.Default.IconPath;
            txtPreviewImage.Text = Settings.Default.PreviewImagePath;

            txtDestination.Text = Settings.Default.SelectedPath;
        }
        private void SaveSettings()
        {
            Settings.Default.Import = chkImport.Checked;
            Settings.Default.TemplateName = txtTemplateName.Text;
            Settings.Default.Description = txtDescription.Text;
            Settings.Default.LanguageTag = LanguageTag;
            Settings.Default.ProjectType = ProjectType;
            Settings.Default.PlatformTag = cboPlatformTag.Text;
            Settings.Default.ProjectTypeTag = cboProjectTypeTag.Text;
            Settings.Default.SelectedPath = txtDestination.Text;
            Settings.Default.ProjectSubType = txtProjectSubType.Text;

            Settings.Default.IconPath = txtIcon.Text;
            Settings.Default.PreviewImagePath = txtPreviewImage.Text;
            Settings.Default.Save();
        }

        private void ProjectSelectorForm_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTemplateName.Text))
            {
                txtTemplateName.Text = txtDefaultName.Text ?? "Template Name";
            }

            if (string.IsNullOrWhiteSpace(txtDescription.Text))
            {
                txtDescription.Text = txtTemplateName.Text + " description";
            }

            SetSolutionItems();
        }

        private void SetSolutionItems()
        {
            lstItems.Items.Clear();

            foreach (var solutionItem in SolutionItems)
            {
                string s = string.Join(" \\ ", solutionItem.Children.Select(x => x.Name));

                ListViewItem lvi = new ListViewItem();
                lvi.Name = solutionItem.Name;
                lvi.Text = solutionItem.Name;
                if (!string.IsNullOrEmpty(s))
                    lvi.Text += "\\" + s;
                lvi.ImageIndex = solutionItem.IsProject ? 0 : 1;
                lstItems.Items.Add(lvi);
            }
        }

        private void SetFolderSolutionItems()
        {
            lstItems.Items.Clear();

            foreach (var solutionItem in FolderSolutionItems)
            {
                ListViewItem lvi = new ListViewItem();
                lvi.Name = solutionItem.Name;
                lvi.Text = solutionItem.Name;
                lvi.ImageIndex = solutionItem.IsProject ? 0 : 1;
                lstItems.Items.Add(lvi);
            }
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            var errors = new List<string>();
            var projectTemplateCount = lstItems.CheckedItems.Count;

            if (string.IsNullOrWhiteSpace(txtTemplateName.Text))
            {
                errors.Add("Missing Template Name");
            }
            if (string.IsNullOrWhiteSpace(txtDefaultName.Text))
            {
                errors.Add("Missing Default Name");
            }
            if (string.IsNullOrWhiteSpace(txtDescription.Text))
            {
                errors.Add("Missing Description");
            }

            if (!txtDestination.Text.DirectoryExists())
            {
                errors.Add(@"Destination folder doesn't exist.");
            }

            if (lstItems.CheckedItems.Count == 0)
            {
                errors.Add(@"You have to select at least one project.");
            }

            if (errors.Count != 0)
            {
                var errorMessage = string.Join(Environment.NewLine, errors);
                MessageBox.Show(errorMessage, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MessageBox.Show($"Are you sure you wan't to generate {projectTemplateCount} project templates", "Generate Templates", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation)
                != DialogResult.Yes)
            {
                return;
            }

            SelectedSolutionItems.Clear();
            foreach (ListViewItem lvi in lstItems.CheckedItems)
            {
                var item = rbUseSolution.Checked
                    ? SolutionItems.Single(x => x.Name.Equals(lvi.Name))
                    : FolderSolutionItems.Single(x => x.Name.Equals(lvi.Name));
                SelectedSolutionItems.Add(item);
            }

            SaveSettings();

            DialogResult = DialogResult.OK;
        }

        private void btnDestBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog
            {
                Description = @"Select templates destination folder",
                ShowNewFolderButton = true,
                RootFolder = Environment.SpecialFolder.Desktop,
                SelectedPath = Settings.Default.SelectedPath
            };

            try
            {
                if (folderBrowser.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
            }
            catch (Exception)
            {
                folderBrowser.SelectedPath = string.Empty;
                if (folderBrowser.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
            }

            txtDestination.Text = folderBrowser.SelectedPath;
        }

        private void btnIcon_Click(object sender, EventArgs e)
        {
            DialogResult dlgResult = openFileDialog1.ShowDialogPath(txtIcon.Text);
            if (dlgResult != DialogResult.OK)
            {
                return;
            }

            txtIcon.Text = openFileDialog1.FileName;
        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            DialogResult dlgResult = openFileDialog1.ShowDialogPath(txtPreviewImage.Text);
            if (dlgResult != DialogResult.OK)
            {
                return;
            }

            txtPreviewImage.Text = openFileDialog1.FileName;
        }

        private void rbUseSolution_CheckedChanged(object sender, EventArgs e)
        {
            if (!rbUseSolution.Checked)
                return;

            SetSolutionItems();
        }

        private void rbUseFolder_CheckedChanged(object sender, EventArgs e)
        {
            if (!rbUseFolder.Checked)
                return;
            FolderSolutionItems.Clear();

            if (txtDestination.Text.DirectoryExists())
            {
                Application.DoEvents();
                FolderSolutionItems = _generatorService.GetSolutionItemsFromFolder(txtDestination.Text);
            }

            SetFolderSolutionItems();
        }
    }
}
