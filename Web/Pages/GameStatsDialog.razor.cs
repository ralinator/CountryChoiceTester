using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using Web.Data;

namespace Web.Pages
{
    public partial class GameStatsDialog
    {
        [CascadingParameter]
        private MudDialogInstance MudDialog { get; set; } = null!;
        [Inject]
        private IDbContextFactory<ApplicationDbContext> DbContextFactory { get; set; } = null!;


        private List<GameRecord> _gameRecords = new();
        protected override async Task OnInitializedAsync()
        {
            await using var db = await DbContextFactory.CreateDbContextAsync();
            _gameRecords = await db.GameRecords.ToListAsync();
        }
    }
}