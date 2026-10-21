using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace BlazorPortal.Components.Layout
{
    public partial class NavMenu
    {
        private string? currentUrl;
        private string? userName;
        private int notiCount = 0;






        protected override async Task OnInitializedAsync()
        {
            var authState = await authProvider.GetAuthenticationStateAsync();
            userName = authState?.User?.Identity?.Name;

            currentUrl = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
            NavigationManager.LocationChanged += OnLocationChanged;
        }









        private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
        {
            currentUrl = NavigationManager.ToBaseRelativePath(e.Location);

            using var context = contextFactory.CreateDbContext();
            var userId = context.vwAppUsers.Where(x => x.UserName == userName).Select(x => x.Id).FirstOrDefault();
            notiCount = context.Notifications.Where(x => x.AppUserId == userId && x.IsRead == false).Count();

            StateHasChanged();
        }








        public void Dispose()
        {
            NavigationManager.LocationChanged -= OnLocationChanged;
        }
    }
}