using JournalApp.Services;
using JournalApp.Data;
using JournalApp.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace JournalApp.Components.Pages
{
    public partial class Dashboard
    {
        [Inject] public AuthenticationStateService AuthState { get; set; } = default!;
        [Inject] public AuthenticationService AuthenticationService { get; set; } = default!;
        [Inject] public JournalService JournalService { get; set; } = default!;
        [Inject] public NavigationManager Navigation { get; set; } = default!;
        [Inject] public AnalyticsService AnalyticsService { get; set; } = default!;
        [Inject] public ISnackbar Snackbar { get; set; } = default!;
        [Inject] public IDialogService DialogService { get; set; } = default!;
        
        private AnalyticsData? analytics;
        private List<JournalEntry> recentEntries = new();
        private bool isLoading = true;
        private string errorMessage = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            try 
            {
                if (!AuthState.IsAuthenticated)
                {
                    Navigation.NavigateTo("/login");
                    return;
                }

            var userId = AuthState.GetCurrentUserId();
            if (!userId.HasValue)
            {
                Navigation.NavigateTo("/login");
                return;
            }

            await LoadDashboardDataAsync();
            }
            catch (Exception ex)
            {
                errorMessage = "Failed to load dashboard data. Please try again later.";
                Console.WriteLine($"[ERROR] Dashboard.OnInitializedAsync: {ex.Message}");
            }
        }

        private async Task LoadDashboardDataAsync()
        {
            if (!AuthState.IsAuthenticated)
                return;
                
            isLoading = true;
            
            try
            {
                var userId = AuthState.GetCurrentUserId();
                if (userId.HasValue)
                {
                    analytics = await AnalyticsService.GetAnalyticsAsync(userId.Value);
                    recentEntries = await JournalService.GetAllEntriesAsync(userId.Value, page: 1, pageSize: 5);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading dashboard: {ex.Message}");
            }
            finally
            {
                isLoading = false;
            }
        }

        private async Task HandleEntryClick(JournalEntry entry)
        {
            if (string.IsNullOrEmpty(entry.Pin))
            {
                ViewEntry(entry.Id);
                return;
            }

            var parameters = new DialogParameters { ["TargetPin"] = entry.Pin };
            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall };
            var dialog = await DialogService.ShowAsync<JournalPinChallengeDialog>("Unlock Journal", parameters, options);
            var result = await dialog.Result;

            if (result != null && !result.Canceled)
            {
                ViewEntry(entry.Id);
            }
        }

        private void ViewEntry(int entryId)
        {
            Navigation.NavigateTo($"/view-entry/{entryId}");
        }

        private string GetMoodEmoji(string mood)
        {
            return mood?.ToLower() switch
            {
                "happy" => "😊",
                "excited" => "🤩",
                "content" => "😌",
                "calm" => "😇",
                "sad" => "😢",
                "anxious" => "😰",
                "angry" => "😠",
                "frustrated" => "😤",
                "tired" => "😴",
                "energetic" => "⚡",
                "grateful" => "🙏",
                "loved" => "❤️",
                "hopeful" => "🌟",
                "confused" => "😕",
                "lonely" => "😔",
                "stressed" => "😫",
                "peaceful" => "☮️",
                "inspired" => "💡",
                "proud" => "🏆",
                "disappointed" => "😞",
                _ => "😐"
            };
        }
    }
}
