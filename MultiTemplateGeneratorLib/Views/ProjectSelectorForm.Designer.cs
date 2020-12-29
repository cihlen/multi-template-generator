
namespace MultiTemplateGeneratorLib.Views
{
    partial class ProjectSelectorForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProjectSelectorForm));
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.lstItems = new System.Windows.Forms.ListView();
            this.label2 = new System.Windows.Forms.Label();
            this.txtTemplateName = new System.Windows.Forms.TextBox();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtDefaultName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.txtIcon = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtPreviewImage = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtDestination = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.btnDestBrowse = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.btnIcon = new System.Windows.Forms.Button();
            this.btnPreview = new System.Windows.Forms.Button();
            this.txtProjectSubType = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.label12 = new System.Windows.Forms.Label();
            this.cboLanguageTag = new System.Windows.Forms.ComboBox();
            this.label13 = new System.Windows.Forms.Label();
            this.rbUseSolution = new System.Windows.Forms.RadioButton();
            this.rbUseFolder = new System.Windows.Forms.RadioButton();
            this.chkImport = new System.Windows.Forms.CheckBox();
            this.cboProjectTypeTag = new MultiTemplateGeneratorLib.Controls.CheckedComboBox();
            this.cboPlatformTag = new MultiTemplateGeneratorLib.Controls.CheckedComboBox();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(553, 487);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 32);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnGenerate
            // 
            this.btnGenerate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGenerate.Location = new System.Drawing.Point(459, 487);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(88, 32);
            this.btnGenerate.TabIndex = 0;
            this.btnGenerate.Text = "Generate";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // lstItems
            // 
            this.lstItems.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstItems.CheckBoxes = true;
            this.lstItems.HideSelection = false;
            this.lstItems.Location = new System.Drawing.Point(13, 284);
            this.lstItems.Name = "lstItems";
            this.lstItems.Size = new System.Drawing.Size(628, 192);
            this.lstItems.TabIndex = 4;
            this.lstItems.UseCompatibleStateImageBehavior = false;
            this.lstItems.View = System.Windows.Forms.View.List;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(112, 17);
            this.label2.TabIndex = 5;
            this.label2.Text = "Template Name:";
            // 
            // txtTemplateName
            // 
            this.txtTemplateName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTemplateName.Location = new System.Drawing.Point(141, 12);
            this.txtTemplateName.MaxLength = 255;
            this.txtTemplateName.Name = "txtTemplateName";
            this.txtTemplateName.Size = new System.Drawing.Size(500, 22);
            this.txtTemplateName.TabIndex = 6;
            this.toolTip1.SetToolTip(this.txtTemplateName, "The name of the template");
            // 
            // txtDescription
            // 
            this.txtDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDescription.Location = new System.Drawing.Point(141, 68);
            this.txtDescription.MaxLength = 200;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(500, 22);
            this.txtDescription.TabIndex = 8;
            this.toolTip1.SetToolTip(this.txtDescription, "Specify the description that will appear under the template title");
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 71);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(83, 17);
            this.label3.TabIndex = 7;
            this.label3.Text = "Description:";
            // 
            // txtDefaultName
            // 
            this.txtDefaultName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDefaultName.Location = new System.Drawing.Point(141, 40);
            this.txtDefaultName.MaxLength = 100;
            this.txtDefaultName.Name = "txtDefaultName";
            this.txtDefaultName.Size = new System.Drawing.Size(500, 22);
            this.txtDefaultName.TabIndex = 10;
            this.toolTip1.SetToolTip(this.txtDefaultName, "the default name for the solution");
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 43);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(98, 17);
            this.label4.TabIndex = 9;
            this.label4.Text = "Default Name:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(13, 129);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(121, 17);
            this.label6.TabIndex = 13;
            this.label6.Text = "Project Type Tag:";
            // 
            // txtIcon
            // 
            this.txtIcon.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtIcon.Location = new System.Drawing.Point(141, 155);
            this.txtIcon.MaxLength = 255;
            this.txtIcon.Name = "txtIcon";
            this.txtIcon.Size = new System.Drawing.Size(464, 22);
            this.txtIcon.TabIndex = 16;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 158);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(101, 17);
            this.label7.TabIndex = 15;
            this.label7.Text = "Template Icon:";
            // 
            // txtPreviewImage
            // 
            this.txtPreviewImage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPreviewImage.Location = new System.Drawing.Point(141, 183);
            this.txtPreviewImage.MaxLength = 255;
            this.txtPreviewImage.Name = "txtPreviewImage";
            this.txtPreviewImage.Size = new System.Drawing.Size(464, 22);
            this.txtPreviewImage.TabIndex = 18;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(12, 186);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(103, 17);
            this.label8.TabIndex = 17;
            this.label8.Text = "Preview Image:";
            // 
            // txtDestination
            // 
            this.txtDestination.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDestination.Location = new System.Drawing.Point(141, 211);
            this.txtDestination.MaxLength = 255;
            this.txtDestination.Name = "txtDestination";
            this.txtDestination.Size = new System.Drawing.Size(464, 22);
            this.txtDestination.TabIndex = 20;
            this.toolTip1.SetToolTip(this.txtDestination, "Path to the destination folder for the multi-project templates ");
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(12, 214);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(83, 17);
            this.label9.TabIndex = 19;
            this.label9.Text = "Destination:";
            // 
            // btnDestBrowse
            // 
            this.btnDestBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDestBrowse.Location = new System.Drawing.Point(611, 209);
            this.btnDestBrowse.Name = "btnDestBrowse";
            this.btnDestBrowse.Size = new System.Drawing.Size(30, 24);
            this.btnDestBrowse.TabIndex = 21;
            this.btnDestBrowse.Text = "...";
            this.btnDestBrowse.UseVisualStyleBackColor = true;
            this.btnDestBrowse.Click += new System.EventHandler(this.btnDestBrowse_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.DefaultExt = "ico";
            this.openFileDialog1.FileName = "openFile";
            this.openFileDialog1.Filter = "Image files (*.ico,*.png,*.jpg,*.jpeg,*.bmp)|*.ico;*.png;*.jpg;*.jpeg;*.bmp";
            this.openFileDialog1.SupportMultiDottedExtensions = true;
            this.openFileDialog1.Title = "Select Image";
            // 
            // btnIcon
            // 
            this.btnIcon.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnIcon.Location = new System.Drawing.Point(611, 153);
            this.btnIcon.Name = "btnIcon";
            this.btnIcon.Size = new System.Drawing.Size(30, 26);
            this.btnIcon.TabIndex = 22;
            this.btnIcon.Text = "...";
            this.btnIcon.UseVisualStyleBackColor = true;
            this.btnIcon.Click += new System.EventHandler(this.btnIcon_Click);
            // 
            // btnPreview
            // 
            this.btnPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPreview.Location = new System.Drawing.Point(611, 181);
            this.btnPreview.Name = "btnPreview";
            this.btnPreview.Size = new System.Drawing.Size(30, 26);
            this.btnPreview.TabIndex = 23;
            this.btnPreview.Text = "...";
            this.btnPreview.UseVisualStyleBackColor = true;
            this.btnPreview.Click += new System.EventHandler(this.btnPreview_Click);
            // 
            // txtProjectSubType
            // 
            this.txtProjectSubType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtProjectSubType.Location = new System.Drawing.Point(403, 126);
            this.txtProjectSubType.MaxLength = 30;
            this.txtProjectSubType.Name = "txtProjectSubType";
            this.txtProjectSubType.Size = new System.Drawing.Size(238, 22);
            this.txtProjectSubType.TabIndex = 25;
            this.toolTip1.SetToolTip(this.txtProjectSubType, "Optional: ");
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(274, 129);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(121, 17);
            this.label10.TabIndex = 24;
            this.label10.Text = "Project Sub Type:";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(274, 97);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(93, 17);
            this.label12.TabIndex = 28;
            this.label12.Text = "Platform Tag:";
            // 
            // cboLanguageTag
            // 
            this.cboLanguageTag.FormattingEnabled = true;
            this.cboLanguageTag.Items.AddRange(new object[] {
            "C#",
            "C++",
            "F#",
            "Java",
            "JavaScript",
            "Python",
            "Query Language",
            "TypeScript",
            "Visual Basic"});
            this.cboLanguageTag.Location = new System.Drawing.Point(141, 96);
            this.cboLanguageTag.Name = "cboLanguageTag";
            this.cboLanguageTag.Size = new System.Drawing.Size(126, 24);
            this.cboLanguageTag.TabIndex = 32;
            this.cboLanguageTag.Text = "C#";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(12, 99);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(76, 17);
            this.label13.TabIndex = 31;
            this.label13.Text = "Language:";
            // 
            // rbUseSolution
            // 
            this.rbUseSolution.AutoSize = true;
            this.rbUseSolution.Location = new System.Drawing.Point(16, 249);
            this.rbUseSolution.Name = "rbUseSolution";
            this.rbUseSolution.Size = new System.Drawing.Size(242, 21);
            this.rbUseSolution.TabIndex = 36;
            this.rbUseSolution.TabStop = true;
            this.rbUseSolution.Text = "Use projects from current solution";
            this.rbUseSolution.UseVisualStyleBackColor = true;
            this.rbUseSolution.CheckedChanged += new System.EventHandler(this.rbUseSolution_CheckedChanged);
            // 
            // rbUseFolder
            // 
            this.rbUseFolder.AutoSize = true;
            this.rbUseFolder.Location = new System.Drawing.Point(301, 249);
            this.rbUseFolder.Name = "rbUseFolder";
            this.rbUseFolder.Size = new System.Drawing.Size(300, 21);
            this.rbUseFolder.TabIndex = 37;
            this.rbUseFolder.TabStop = true;
            this.rbUseFolder.Text = "Use existing templates in Destination folder";
            this.rbUseFolder.UseVisualStyleBackColor = true;
            this.rbUseFolder.CheckedChanged += new System.EventHandler(this.rbUseFolder_CheckedChanged);
            // 
            // chkImport
            // 
            this.chkImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.chkImport.AutoSize = true;
            this.chkImport.Location = new System.Drawing.Point(151, 494);
            this.chkImport.Name = "chkImport";
            this.chkImport.Size = new System.Drawing.Size(302, 21);
            this.chkImport.TabIndex = 39;
            this.chkImport.Text = "Import generated template to Visual Studio ";
            this.chkImport.UseVisualStyleBackColor = true;
            // 
            // cboProjectTypeTag
            // 
            this.cboProjectTypeTag.CheckOnClick = true;
            this.cboProjectTypeTag.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.cboProjectTypeTag.DropDownHeight = 1;
            this.cboProjectTypeTag.FormattingEnabled = true;
            this.cboProjectTypeTag.IntegralHeight = false;
            this.cboProjectTypeTag.Location = new System.Drawing.Point(141, 126);
            this.cboProjectTypeTag.Name = "cboProjectTypeTag";
            this.cboProjectTypeTag.Size = new System.Drawing.Size(127, 23);
            this.cboProjectTypeTag.TabIndex = 38;
            this.cboProjectTypeTag.ValueSeparator = ", ";
            // 
            // cboPlatformTag
            // 
            this.cboPlatformTag.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboPlatformTag.CheckOnClick = true;
            this.cboPlatformTag.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.cboPlatformTag.DropDownHeight = 1;
            this.cboPlatformTag.FormattingEnabled = true;
            this.cboPlatformTag.IntegralHeight = false;
            this.cboPlatformTag.Location = new System.Drawing.Point(403, 96);
            this.cboPlatformTag.Name = "cboPlatformTag";
            this.cboPlatformTag.Size = new System.Drawing.Size(238, 23);
            this.cboPlatformTag.TabIndex = 35;
            this.cboPlatformTag.ValueSeparator = ", ";
            // 
            // ProjectSelectorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(653, 531);
            this.Controls.Add(this.chkImport);
            this.Controls.Add(this.cboProjectTypeTag);
            this.Controls.Add(this.rbUseFolder);
            this.Controls.Add(this.rbUseSolution);
            this.Controls.Add(this.cboPlatformTag);
            this.Controls.Add(this.cboLanguageTag);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.txtProjectSubType);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.btnPreview);
            this.Controls.Add(this.btnIcon);
            this.Controls.Add(this.btnDestBrowse);
            this.Controls.Add(this.txtDestination);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.txtPreviewImage);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.txtIcon);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtDefaultName);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtDescription);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtTemplateName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lstItems);
            this.Controls.Add(this.btnGenerate);
            this.Controls.Add(this.btnCancel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1280, 768);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(534, 400);
            this.Name = "ProjectSelectorForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Select Projects";
            this.Load += new System.EventHandler(this.ProjectSelectorForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.ListView lstItems;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtTemplateName;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtDefaultName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtIcon;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtPreviewImage;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtDestination;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button btnDestBrowse;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button btnIcon;
        private System.Windows.Forms.Button btnPreview;
        private System.Windows.Forms.TextBox txtProjectSubType;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.ComboBox cboLanguageTag;
        private System.Windows.Forms.Label label13;
        private Controls.CheckedComboBox cboPlatformTag;
        private System.Windows.Forms.RadioButton rbUseSolution;
        private System.Windows.Forms.RadioButton rbUseFolder;
        private Controls.CheckedComboBox cboProjectTypeTag;
        private System.Windows.Forms.CheckBox chkImport;
    }
}