using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Web.Pages
{
    public partial class HelpDialog
    {
        [CascadingParameter]
        private MudDialogInstance MudDialog { get; set; } = null!;
    }
}