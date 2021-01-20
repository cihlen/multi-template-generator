using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace MultiTemplateGenerator.Lib.Models
{
    public abstract class XmlTemplateItemBase
    {
        protected XmlTemplateItemBase(string tagName)
        {
            TagName = tagName;
        }

        public virtual string TagName { get; }

        private readonly List<string> _excludedProperties = new List<string> { "Content", "TagName", "Children" };

        private readonly List<Type> _validTypes = new List<Type>()
            {typeof(string), typeof(bool), typeof(int), typeof(long), typeof(short)};

        public virtual string XmlStartTag()
        {
            var xml = new StringBuilder($"<{TagName}");
            foreach (var propertyInfo in GetType().GetProperties()
                .OrderBy(x => x.Name)
                .Where(x => !_excludedProperties.Contains(x.Name)
                            && _validTypes.Contains(x.PropertyType)))
            {
                var val = propertyInfo.GetValue(this, null);
                if (val != null)
                {
                    if (val is bool)
                        val = val.ToString().ToLower();

                    xml.Append($" {propertyInfo.Name}=\"{val}\"");
                }
            }

            xml.Append(">");
            return xml.ToString();
        }

        public virtual string XmlEndTag()
        {
            return $"</{TagName}>";
        }

        public virtual string XmlTag()
        {
            var xml = new StringBuilder(XmlStartTag());
            var content = this.GetType().GetProperties().SingleOrDefault(x => x.Name.Equals("Content"))?.GetValue(this, null);
            if (content != null)
            {
                xml.Append(content);
            }
            xml.Append(XmlEndTag());
            return xml.ToString();
        }

        public XmlNodeList ReadXmlNodeList(string xmlFile)
        {
            XmlDocument xmlDoc = new XmlDocument();
            using var fs = File.OpenRead(xmlFile);
            xmlDoc.Load(fs);

            var contentNode = xmlDoc.SelectSingleNode(TagName);

            if (contentNode == null)
            {
                throw new ArgumentNullException(nameof(contentNode), @$"Cannot find {TagName} node.");
            }

            return contentNode.ChildNodes;
        }
    }
}