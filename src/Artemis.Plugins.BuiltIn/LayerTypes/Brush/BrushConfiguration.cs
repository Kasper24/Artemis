﻿using Artemis.Core.Plugins.Interfaces;

namespace Artemis.Plugins.BuiltIn.LayerTypes.Brush
{
    public class BrushConfiguration : ILayerTypeConfiguration
    {
        public System.Windows.Media.Brush Brush { get; set; }
    }
}