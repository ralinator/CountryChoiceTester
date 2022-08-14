using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using MudBlazor;
using System.Runtime.InteropServices;
using System.Text.Json;
using Web.Data;
using Web.Services;

namespace Web.Pages
{
    public partial class Index
    {
        private const int _numberOfCountriesToGuess = 9;
        private const string _unsetIdentifier = "Unset";

        private List<DropItem> _items = new();
        private List<List<DropItem>> _previousAttemptsToday = new();
        private List<GameRecord> _gameRecords = new();
        private MudDropContainer<DropItem>? _dropContainerReference;
        private MudTabs? _tabsReference = null!;
        private GameData _gameData = null!;

        private bool IsSubmissionValid =>
            _items.All(q => q.Identifier is not _unsetIdentifier)
            && IsTodaysGameSolved is false;

        private bool IsTodaysGameSolved => _gameRecords.Any(q => q.Date.Date == DateTime.Today);
        public class DropItem
        {
            public Country Country { get; init; } = null!;
            public string Identifier { get; set; } = _unsetIdentifier;
            public int OrderIndex { get; set; }
            public bool IsCorrect => Identifier == Country.Code;
        }
        [Inject]
        private IGameDataService GameDataService { get; set; } = null!;
        [Inject]
        private IJSRuntime JSRuntime { get; set; } = null!;
        [Inject]
        private IDbContextFactory<ApplicationDbContext> DbContextFactory { get; set; } = null!;
        [Inject]
        private IDialogService DialogService { get; set; } = null!;
        [Inject]
        private ISnackbar SnackbarService { get; set; } = null!;
        protected override async Task OnInitializedAsync()
        {
            _gameData = await GameDataService.GetAsync();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Create("browser")))
            {
                // create SQLite database file in browser
                var module = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./dbstorage.js");
                await module.InvokeVoidAsync("createSqLiteFile", DbConstants.SqliteDbFilename);
            }
            await using var db = await DbContextFactory.CreateDbContextAsync();
            //await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();
            _gameRecords = await db.GameRecords.ToListAsync();
            var currentGame = await db.GameStates.OrderBy(q => q.Id).LastOrDefaultAsync(q => q.Date.Date == DateTime.Today);
            await LoadPreviousAttemptsToday();
            if (currentGame is not null)
            {
                _items = JsonSerializer.Deserialize<List<DropItem>>(currentGame.State)!;
            }
            else
            {
                _items = new();
                SetUpNewState();
            }
            await SaveState();
            if (IsTodaysGameSolved)
            {
                OpenDialog();
            }
            if (_items.All(q => q.Identifier != _unsetIdentifier))
            {
                _tabsReference!.ActivatePanel(1);
            }
        }

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);
            _dropContainerReference!.Refresh();
        }

        private void SetUpNewState()
        {
            var random = new Random((int)DateTime.Today.Ticks);
            for (int i = 0; i < _numberOfCountriesToGuess; i++)
            {
                int index;
                do
                {
                    index = random.Next(0, _gameData.Countries.Count);
                }
                while (_items.Any(q => q.Country == _gameData.Countries.ElementAt(index)));
                int orderIndex;
                do
                {
                    orderIndex = random.Next(0, _numberOfCountriesToGuess);
                }
                while (_items.Any(q => q.OrderIndex == orderIndex));
                _items.Add(new DropItem
                {
                    Country = _gameData.Countries[index],
                    Identifier = _unsetIdentifier,
                    OrderIndex = orderIndex
                });
            }
        }

        private async Task SaveState()
        {
            await using var db = await DbContextFactory.CreateDbContextAsync();
            var currentGame = await db.GameStates.OrderBy(q => q.Id).LastOrDefaultAsync(q => q.Date.Date == DateTime.Today);
            if (currentGame is null)
            {
                currentGame = new();
            }
            currentGame.State = JsonSerializer.Serialize(_items);
            db.GameStates.Update(currentGame);
            await db.SaveChangesAsync();
        }

        private async Task ItemUpdated(MudItemDropInfo<DropItem> dropItem)
        {
            if (IsTodaysGameSolved)
            {
                return;
            }
            Func<DropItem, bool> testExpression = q => q.Identifier == dropItem.DropzoneIdentifier;
            if (_items.Any(testExpression))
            {
                var itemAlreadyAssigned = _items.First(testExpression);
                if (itemAlreadyAssigned.Identifier is not _unsetIdentifier)
                {
                    itemAlreadyAssigned.Identifier = dropItem.Item.Identifier;
                }
            }
            dropItem.Item.Identifier = dropItem.DropzoneIdentifier;
            await SaveState();
        }

        private async Task ResetAllItemsAsUnset()
        {
            if (IsTodaysGameSolved)
            {
                return;
            }
            foreach (var item in _items)
            {
                item.Identifier = _unsetIdentifier;
            }
            _dropContainerReference!.Refresh();
            await SaveState();
        }

        private async Task Submit()
        {
            if (IsSubmissionValid is false)
            {
                return;
            }
            await using var db = await DbContextFactory.CreateDbContextAsync();
            if (_items.All(q => q.IsCorrect))
            {
                var attempts = await db.GameStates.CountAsync(q => q.Date.Date == DateTime.Today);
                var gameRecord = new GameRecord
                {
                    Date = DateTime.Today,
                    NumberOfAttemptsRequired = attempts
                };
                db.GameRecords.Update(gameRecord);
                await db.SaveChangesAsync();
                _gameRecords = await db.GameRecords.ToListAsync();
            }
            else
            {
                var state = new GameState();
                db.GameStates.Add(state);
                await db.SaveChangesAsync();
                await SaveState();
            }

            await LoadPreviousAttemptsToday();
            if (IsTodaysGameSolved)
            {
                SnackbarService.Add("All countries correct!", Severity.Success);
                OpenDialog();
            }
            else
            {
                SnackbarService.Add($"{_previousAttemptsToday.Last().Count(q => q.IsCorrect is false)} countries incorrect, please try again",
                    Severity.Error);
            }
            _dropContainerReference!.Refresh();
            _tabsReference!.ActivatePanel(1);
        }

        private async Task LoadPreviousAttemptsToday()
        {
            await using var db = await DbContextFactory.CreateDbContextAsync();
            var attemptsToday = await db.GameStates
                .Where(q => q.Date.Date == DateTime.Today)
                .OrderBy(q => q.Id)
                .Select(q => q.State)
                .ToListAsync();
            // Remove Current Attempt If Not Solved
            if (IsTodaysGameSolved is false && attemptsToday.Any())
            {
                attemptsToday.RemoveAt(attemptsToday.Count - 1);
            }
            _previousAttemptsToday = new();
            foreach (var attempt in attemptsToday)
            {
                var deserializedAttempt = JsonSerializer.Deserialize<List<DropItem>>(attempt)!;
                if (deserializedAttempt.All(q => q.Identifier is not _unsetIdentifier))
                {
                    _previousAttemptsToday.Add(deserializedAttempt);
                }
            }
        }

        private void OpenDialog()
        {
            var options = new DialogOptions
            {
                CloseOnEscapeKey = true,
                FullWidth = true,
                MaxWidth = MaxWidth.Large
            };
            DialogService.Show<GameStatsDialog>("Game Stats", options);
        }
    }
}