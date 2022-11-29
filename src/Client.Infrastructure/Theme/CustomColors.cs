using MudBlazor;

namespace ZANECO.WASM.Client.Infrastructure.Theme;

public static class CustomColors
{
    public static readonly List<string> ThemeColors = new()
    {
        //Light.Primary,
        Colors.Red.Default,
        Colors.Pink.Default,
        Colors.Purple.Default,
        Colors.DeepPurple.Default,
        Colors.Indigo.Default,
        Colors.Blue.Default,
        Colors.LightBlue.Default,
        Colors.Cyan.Default,
        Colors.Teal.Default,
        Colors.Green.Default,
        Colors.LightGreen.Default,
        Colors.Lime.Default,
        Colors.Yellow.Default,
        Colors.Amber.Default,
        Colors.Orange.Default,
        Colors.DeepOrange.Default,
        Colors.BlueGrey.Default,
        Colors.Brown.Default,
        Colors.Grey.Default
    };

    public static class Light
    {
        public const string Primary = "#03a9f4";
        public const string Secondary = "#ffc107";
        public const string Background = "#FFF";
        public const string AppbarBackground = "#FFF";
        public const string AppbarText = "#6e6e6e";
    }

    public static class Dark
    {
        public const string Primary = "#03a9f4";
        public const string Secondary = "#ffc107";
        public const string Background = "#1b1f22";
        public const string AppbarBackground = "#1b1f22";
        public const string DrawerBackground = "#121212";
        public const string Surface = "#202528";
        public const string Disabled = "#545454";
    }
}