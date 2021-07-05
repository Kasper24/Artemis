using System;
using Artemis.Core;

namespace Artemis.UI.Services
{
    public interface ITelemetryService : IArtemisUIService
    {
        PluginSetting<bool> EnableTelemetry { get; }
        void TrackEvent(string category, string action, string name = "", string value = "");
        void TrackPageView(string name);
        void TrackException(Exception exception, string action);
    }
}