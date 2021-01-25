using System;
using System.Collections.Generic;
using System.Linq;
using MultiTemplateGenerator.UI.Models;
using MultiTemplateGenerator.Lib;
using MultiTemplateGenerator.Lib.Models;

namespace MultiTemplateGenerator.UI
{
    public static class ModelExtensions
    {
        public static List<T> GetCheckedItems<T>(this IEnumerable<IChecked> items)
        {
            return items.Where(x => x.IsChecked).Cast<T>().ToList();
        }

        public static List<CheckedItemModel> GetCheckedItems(this IEnumerable<CheckedItemModel> items)
        {
            return items.Where(x => x.IsChecked).ToList();
        }

        public static IList<CheckedItemModel> GetNewCheckedItems(this IList<CheckedItemModel> items, string longText, Action<CheckedItemModel> checkedChanged)
        {
            foreach (var part in longText.GetTags())
            {
                if (!items.Any(x => x.Text.Equals(part, StringComparison.InvariantCultureIgnoreCase)))
                {
                    items.Add(new CheckedItemModel(checkedChanged, part, true));
                }
            }
            return items;
        }

        public static string GetCheckedItemsString(this IEnumerable<CheckedItemModel> items)
        {
            return String.Join(",", items.GetCheckedItems().Select(x => x.Text));
        }

        public static IList<CheckedItemModel> CombineCheckedItems(this IList<string> itemsTexts, string additionalItemsString, Action<CheckedItemModel> checkedChanged)
        {
            var items = new List<CheckedItemModel>(itemsTexts.Select(x => new CheckedItemModel(checkedChanged, x)));
            return items.GetNewCheckedItems(additionalItemsString, checkedChanged);
        }

        public static List<IProjectTemplate> GetTemplatesFlattened(this IEnumerable<IProjectTemplate> projectItems)
        {
            var items = projectItems.ToList();
            var itemsCount = items.Count;

            for (var index = 0; index < itemsCount; index++)
            {
                var item = items[index];
                items.AddRange(GetTemplatesFlattened(item.Children));
            }

            return items;
        }

        public static ProjectTemplateModel GetParentProject(this ProjectTemplateModel template)
        {
            if (template.Parent == null || template.Parent.IsProject)
                return template.Parent;

            return template.Parent.GetParentProject();
        }
    }
}
