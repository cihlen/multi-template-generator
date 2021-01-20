using System.Collections.Generic;
using System.Linq;

namespace MultiTemplateGenerator.Lib.Models
{
    public static class XmlTemplateItemExtensions
    {
        public static IEnumerable<T> GetItems<T>(this IEnumerable<XmlTemplateItemBase> items) 
            where T: XmlTemplateItemBase
        {
            return items.Where(x => x.GetType() == typeof(T)).Cast<T>();
        }

        public static IEnumerable<XmlProjectFolder> GetFolders(this IEnumerable<XmlTemplateItemBase> items)
        {
            return items.GetItems<XmlProjectFolder>();
        }

        public static IEnumerable<XmlProjectItem> GetProjectItems(this IEnumerable<XmlTemplateItemBase> items)
        {
            return items.GetItems<XmlProjectItem>();
        }

        public static IEnumerable<XmlSolutionFolder> GetSolutionFolders(this IEnumerable<XmlTemplateItemBase> items)
        {
            return items.GetItems<XmlSolutionFolder>();
        }

        public static IEnumerable<XmlProjectTemplateLink> GetProjectTemplateLinks(this IEnumerable<XmlTemplateItemBase> items)
        {
            return items.GetItems<XmlProjectTemplateLink>();
        }
        //public static XmlTemplateItemBase ToXmlTemplateItem(this IProjectTemplate projectTemplate)
        //{
        //    XmlTemplateItemBase baseItem = projectTemplate.IsProject ? new 
        //}
        //public static IEnumerable<XmlTemplateItemBase> ToXmlTemplateItems(this IEnumerable<IProjectTemplate> projectTemplates)
        //{
        //    foreach(var projectTemplate in projectTemplates)
        //    {
        //        var item = projectTemplate.ToXmlTemplateItem();
        //    }
        //}
    }
}
