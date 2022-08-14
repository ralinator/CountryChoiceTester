using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using System.Timers;
using Web.Data;
using Timer = System.Timers.Timer;

namespace Web.Pages
{
    public partial class GameStatsDialog : IDisposable
    {
        [CascadingParameter]
        private MudDialogInstance MudDialog { get; set; } = null!;
        [Inject]
        private IDbContextFactory<ApplicationDbContext> DbContextFactory { get; set; } = null!;


        private List<GameRecord> _gameRecords = new();

        private Timer _timer = null!;
        private int _hoursRemaining;
        private int _minutesRemaining;
        private int _secondsRemaining;

        protected override async Task OnInitializedAsync()
        {
            await using var db = await DbContextFactory.CreateDbContextAsync();
            _gameRecords = await db.GameRecords
                .OrderByDescending(q => q.Date)
                .ToListAsync();
            StartTimer();
        }

        private void StartTimer()
        {
            _timer = new Timer(1000);
            _timer.Elapsed += OnTimerTick;
            _timer.Start();
        }

        private void OnTimerTick(object? source, ElapsedEventArgs e)
        {
            var timeRemaining = DateTime.Today.AddDays(1) - DateTime.Now;
            _hoursRemaining = timeRemaining.Hours;
            _minutesRemaining = timeRemaining.Minutes;
            _secondsRemaining = timeRemaining.Seconds;
            StateHasChanged();
        }

        public void Dispose()
        {
            _timer?.Stop();
            _timer?.Dispose();
        }
    }
}