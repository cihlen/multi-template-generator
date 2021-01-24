using System.Collections.Generic;
using System.Linq;
using MultiTemplateGenerator.UI.Models;
using MultiTemplateGenerator.Lib;
using MultiTemplateGenerator.Lib.Models;

namespace MultiTemplateGenerator.UI
{
    public static class ProjectTemplateConverter
    {
        public static readonly IEnumerable<string> PropertyExcludes =
            new[] { "Parent", "Children", "IsProject", "ProjectItems" };

        public static void CopyTemplateProperties(this IProjectTemplate source, IProjectTemplate target)
        {
            source.CopyPropertiesTo(target, PropertyExcludes);
        }

        public static ProjectTemplateModel ToModel(this IProjectTemplate item, ProjectTemplateModel parent)
        {
            var model = new ProjectTemplateModel(item.IsProject, parent, null);

            item.CopyTemplateProperties(model);
            foreach (var itemChild in item.Children)
            {
                model.Children.Add(itemChild.ToModel(model));
            }

            model.SetItemImage();

            return model;
        }

        public static IEnumerable<ProjectTemplateModel> ToModels(this IEnumerable<IProjectTemplate> items, ProjectTemplateModel parent = null)
        {
            foreach (var item in items)
            {
                yield return item.ToModel(parent);
            }
        }

        public static ProjectTemplate ToProjectTemplate(this ProjectTemplateModel item)
        {
            var model = new ProjectTemplate(item.IsProject);

            item.CopyTemplateProperties(model);
            model.IsHidden = true;

            return model;
        }

        public static IEnumerable<IProjectTemplate> ConvertCheckedProjectTemplates(this IEnumerable<ProjectTemplateModel> items)
        {
            var checkedItems = items.Where(x => x.IsChecked).ToList();
            var projectItems = new List<IProjectTemplate>();

            foreach (var checkedItem in checkedItems)
            {
                var projectItem = checkedItem.ToProjectTemplate();
                projectItems.Add(projectItem);
                projectItem.Children.AddRange(ConvertCheckedProjectTemplates(checkedItem.Children.Cast<ProjectTemplateModel>()));
            }

            return projectItems;
        }
    }
}
