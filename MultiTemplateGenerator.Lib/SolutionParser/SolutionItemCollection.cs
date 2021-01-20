using System.Collections.Generic;
using System.Linq;

namespace MultiTemplateGenerator.Lib.SolutionParser
{
    public class SolutionItemCollection : List<SolutionProjectItem>
    {
        public SolutionItemCollection(List<SolutionProjectItem> items, Dictionary<string, string> nestedItems)
        {
            AddRange(items);

            NestedItems = nestedItems;
        }

        public Dictionary<string, string> NestedItems { get; set; }

        private void ClearChildren(IEnumerable<SolutionProjectItem> items)
        {
            foreach (var item in items)
            {

            }
        }

        public List<SolutionProjectItem> GetSortedHierarchy()
        {
            var list = new List<SolutionProjectItem>(this);
            foreach (var item in list)
            {
                item.Children.Clear();
            }

            foreach (var nestedItem in NestedItems)
            {
                var child = this.Single(x => x.Uid.Equals(nestedItem.Key));
                var parent = this.Single(x => x.Uid.Equals(nestedItem.Value));
                parent.Children.Add(child);
                child.Parent = parent;
                list.Remove(child);
            }

            return list.OrderBy(x => x.Name).ToList();
        }
    }
}
