using MudBlazor;

namespace RealDesign2.Client.Theme;

public static class AppTheme
{
    public static readonly MudTheme Default = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#44abdf",
            Background = "#F9FAFB",
            Surface = "#FFFFFF",
            AppbarBackground = "#44abdf"
        },
        Typography = new Typography
        {
            Default = new DefaultTypography
            {
                FontFamily = ["Inter", "Roboto", "sans-serif"]
            }
        }
    };
}
