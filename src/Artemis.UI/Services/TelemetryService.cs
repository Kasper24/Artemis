using System;
using System.Reflection;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Screens;
using Piwik.Tracker;

namespace Artemis.UI.Services
{
    public class TelemetryService : ITelemetryService
    {
        private PiwikTracker _client;

        public TelemetryService(ISettingsService settings)
        {
            EnableTelemetry = settings.GetSetting("UI.EnableTelemetry", true);
            EnableTelemetry.SettingChanged += EnableTelemetryOnSettingChanged;
            if (EnableTelemetry.Value)
                InitializeClient();
        }

        private void EnableTelemetryOnSettingChanged(object? sender, EventArgs e)
        {
            if (EnableTelemetry.Value)
                InitializeClient();
            else
                _client = null;
        }

        private void InitializeClient()
        {
            AssemblyInformationalVersionAttribute versionAttribute = typeof(RootViewModel).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

            _client = new PiwikTracker(4, "https://stats.artemis-rgb.com/");
            _client.SetCustomTrackingParameter("dimension1", $"{versionAttribute?.InformationalVersion} build {Constants.BuildInfo.BuildNumberDisplay}");
            _client.SetCustomTrackingParameter("dimension2", Environment.OSVersion.ToString());
        }

        public PluginSetting<bool> EnableTelemetry { get; }

        public void TrackEvent(string category, string action, string name = "", string value = "")
        {
            if (!EnableTelemetry.Value || _client == null)
                return;

            _client.DoTrackEvent(category, action, name, value);
        }

        public void TrackPageView(string name)
        {
            if (!EnableTelemetry.Value || _client == null)
                return;

            _client.DoTrackPageView(name);
        }

        public void TrackException(Exception exception, string action)
        {
            if (!EnableTelemetry.Value || _client == null)
                return;

            _client.DoTrackEvent("Exception", action, exception.Message, exception.ToString());
        }
    }
}