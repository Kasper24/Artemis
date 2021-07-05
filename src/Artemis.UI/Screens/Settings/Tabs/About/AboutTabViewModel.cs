using System.Reflection;
using System.Windows.Navigation;
using Artemis.Core;
using Artemis.UI.Services;
using Stylet;

namespace Artemis.UI.Screens.Settings.Tabs.About
{
    public class AboutTabViewModel : Screen
    {
        private readonly ITelemetryService _telemetryService;
        private string _version;

        public AboutTabViewModel(ITelemetryService telemetryService)
        {
            _telemetryService = telemetryService;
            DisplayName = "ABOUT";
        }

        public string Version
        {
            get => _version;
            set => SetAndNotify(ref _version, value);
        }

        public void OpenHyperlink(object sender, RequestNavigateEventArgs e)
        {
            Core.Utilities.OpenUrl(e.Uri.AbsoluteUri);
        }
        
        public void OpenUrl(string url)
        {
            Core.Utilities.OpenUrl(url);
        }

        #region Overrides of Screen

        /// <inheritdoc />
        protected override void OnInitialActivate()
        {
            _telemetryService.TrackPageView("Settings.About");
            base.OnInitialActivate();
        }

        /// <inheritdoc />
        protected override void OnActivate()
        {
            AssemblyInformationalVersionAttribute versionAttribute = typeof(RootViewModel).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            Version = $"Version {versionAttribute?.InformationalVersion} build {Constants.BuildInfo.BuildNumberDisplay}";
            
            base.OnActivate();
        }

        #endregion
    }
}