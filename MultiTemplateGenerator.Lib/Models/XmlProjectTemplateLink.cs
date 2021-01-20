namespace MultiTemplateGenerator.Lib.Models
{
    public class XmlProjectTemplateLink : XmlTemplateItemBase
    {
        public XmlProjectTemplateLink() : base("ProjectTemplateLink")
        {
        }

        public string Content { get; set; }
        public string ProjectName { get; set; }
        public string CopyParameters { get; set; }
    }
}