using MudBlazor;

namespace RealDesign2.Client.Theme;

public static class AppTheme
{
    public static readonly MudTheme Default = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#2563EB",
            Secondary = "#F43F5E",
            Background = "#F6F8FB",
            Surface = "#FFFFFF",
            AppbarBackground = "#FFFFFF",
            AppbarText = "#0F172A",
            TextPrimary = "#0F172A",
            TextSecondary = "#64748B",
            DrawerBackground = "#FFFFFF",
            DrawerText = "#334155",
            LinesDefault = "#E2E8F0",
            TableLines = "#E2E8F0",
            Error = "#EF4444",
            Success = "#16A34A",
            Warning = "#F59E0B"
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
