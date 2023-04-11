using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ZANECO.WASM.Client.Components.Common
{
    public partial class BottomNavMenu : MudComponentBase
    {
        [Parameter] public RenderFragment ChildContent { get; set; }
        [Parameter] public bool ShowMenuTitle { get; set; } = true;
    }
}