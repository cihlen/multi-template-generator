namespace MultiTemplateGenerator.Lib.Models
{
    public class XmlProjectItem : XmlTemplateItemBase
    {
        public XmlProjectItem() : base("ProjectItem")
        {
        }

        public string TargetFileName { get; set; }
        public string ReplaceParameters { get; set; }
        public string OpenInEditor { get; set; }
        public string OpenInWebBrowser { get; set; }
        public string OpenInHelpBrowser { get; set; }
        public string OpenOrder { get; set; }
        public string Content { get; set; }
    }
}