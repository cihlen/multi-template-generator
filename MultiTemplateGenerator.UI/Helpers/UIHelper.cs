using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MultiTemplateGenerator.UI.Helpers
{
    internal static class UIHelper
    {
        internal static bool IsInDesignMode
        {
            get
            {
                var dpDescriptor = DependencyPropertyDescriptor.FromProperty(
                    DesignerProperties.IsInDesignModeProperty, typeof(FrameworkElement));

                return dpDescriptor?.Metadata?.DefaultValue != null
                       && (bool)dpDescriptor.Metadata.DefaultValue;
            }
        }
    }
}
