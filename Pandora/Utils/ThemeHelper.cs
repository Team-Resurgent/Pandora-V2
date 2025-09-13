using Avalonia;
using Avalonia.Media;
using Avalonia.Themes.Fluent;
using System.Reflection;

namespace Pandora.Utils
{
    public class ThemeHelper
    {
        public static ColorPaletteResources CreateLightPalette()
        {
            return new ColorPaletteResources
            {
                Accent = Color.Parse("#ff007300"),
                AltHigh = Colors.White,
                AltLow = Colors.White,
                AltMedium = Colors.White,
                AltMediumHigh = Colors.White,
                AltMediumLow = Colors.White,
                BaseHigh = Colors.Black,
                BaseLow = Color.Parse("#ffcccccc"),
                BaseMedium = Color.Parse("#ff898989"),
                BaseMediumHigh = Color.Parse("#ff5d5d5d"),
                BaseMediumLow = Color.Parse("#ff737373"),
                ChromeAltLow = Color.Parse("#ff5d5d5d"),
                ChromeBlackHigh = Colors.Black,
                ChromeBlackLow = Color.Parse("#ffcccccc"),
                ChromeBlackMedium = Color.Parse("#ff5d5d5d"),
                ChromeBlackMediumLow = Color.Parse("#ff898989"),
                ChromeDisabledHigh = Color.Parse("#ffcccccc"),
                ChromeDisabledLow = Color.Parse("#ff898989"),
                ChromeGray = Color.Parse("#ff737373"),
                ChromeHigh = Color.Parse("#ffcccccc"),
                ChromeLow = Color.Parse("#ffececec"),
                ChromeMedium = Color.Parse("#ffe6e6e6"),
                ChromeMediumLow = Color.Parse("#ffececec"),
                ChromeWhite = Colors.White,
                ListLow = Color.Parse("#ffe6e6e6"),
                ListMedium = Color.Parse("#ffcccccc"),
                RegionColor = Colors.White
            };
        }

        public static ColorPaletteResources CreateDarkPalette()
        {
            return new ColorPaletteResources
            {
                Accent = Color.Parse("#ff007300"),
                AltHigh = Colors.Black,
                AltLow = Colors.Black,
                AltMedium = Colors.Black,
                AltMediumHigh = Colors.Black,
                AltMediumLow = Colors.Black,
                BaseHigh = Colors.White,
                BaseLow = Color.Parse("#ff333333"),
                BaseMedium = Color.Parse("#ff9a9a9a"),
                BaseMediumHigh = Color.Parse("#ffb4b4b4"),
                BaseMediumLow = Color.Parse("#ff676767"),
                ChromeAltLow = Color.Parse("#ffb4b4b4"),
                ChromeBlackHigh = Colors.Black,
                ChromeBlackLow = Color.Parse("#ffb4b4b4"),
                ChromeBlackMedium = Colors.Black,
                ChromeBlackMediumLow = Colors.Black,
                ChromeDisabledHigh = Color.Parse("#ff333333"),
                ChromeDisabledLow = Color.Parse("#ff9a9a9a"),
                ChromeGray = Colors.Gray,
                ChromeHigh = Colors.Gray,
                ChromeLow = Color.Parse("#ff151515"),
                ChromeMedium = Color.Parse("#ff1d1d1d"),
                ChromeMediumLow = Color.Parse("#ff2c2c2c"),
                ChromeWhite = Colors.White,
                ListLow = Color.Parse("#ff1d1d1d"),
                ListMedium = Color.Parse("#ff333333"),
                RegionColor = Colors.Black
            };
        }

        public static Color? GetColorByName(string propertyName)
        {
            Color? result = null;
            var palette = Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Light ? CreateLightPalette() : CreateDarkPalette();
            var prop = typeof(ColorPaletteResources).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.PropertyType == typeof(Color))
            {
                result = prop.GetValue(palette) as Color?;
            }
            return result ?? Colors.Transparent; 
        }

        public static Brush GetBrushByName(string propertyName)
        {
            var color = GetColorByName(propertyName);
            return new SolidColorBrush(color ?? Colors.Transparent);
        }

        public static void ApplyTheme(Application app)
        {
            var fluentTheme = new FluentTheme();
            fluentTheme.Palettes.Add(Avalonia.Styling.ThemeVariant.Light, CreateLightPalette());
            fluentTheme.Palettes.Add(Avalonia.Styling.ThemeVariant.Dark, CreateDarkPalette());
            app.Styles.Add(fluentTheme);
        }
    }
}
