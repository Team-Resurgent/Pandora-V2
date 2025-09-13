using Avalonia.Markup.Xaml;
using System;

namespace Pandora.Utils
{
    public class BrushByNameExtension : MarkupExtension
    {
        public string Name { get; set; }

        public BrushByNameExtension()
        {
            Name = string.Empty;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return ThemeHelper.GetBrushByName(Name);
        }
    }
}
