namespace JournalApp.Components.Layout
{
    public partial class MainLayout
    {
        // Toggles theme via bound flag; keeps provider binding intact.
        private Task ToggleTheme()
        {
            _isDarkMode = !_isDarkMode;
            return InvokeAsync(StateHasChanged);
        }
    }
}
