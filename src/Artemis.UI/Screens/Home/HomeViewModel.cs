using Artemis.UI.Ninject.Factories;
using Artemis.UI.Services;

namespace Artemis.UI.Screens.Home
{
    public class HomeViewModel : MainScreenViewModel
    {
        private readonly ITelemetryService _telemetryService;

        public HomeViewModel(IHeaderVmFactory headerVmFactory, ITelemetryService telemetryService)
        {
            _telemetryService = telemetryService;
            DisplayName = "Home";
            HeaderViewModel = headerVmFactory.SimpleHeaderViewModel(DisplayName);
        }

        protected override void OnInitialActivate()
        {
            _telemetryService.TrackPageView("Home");
            base.OnInitialActivate();
        }
        
        public void OpenUrl(string url)
        {
            Core.Utilities.OpenUrl(url);
        }
    }
}