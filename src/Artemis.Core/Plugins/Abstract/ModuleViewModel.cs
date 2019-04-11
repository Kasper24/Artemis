﻿using Artemis.Core.Plugins.Interfaces;
using Artemis.Core.Plugins.Models;
using Stylet;

namespace Artemis.Core.Plugins.Abstract
{
    public abstract class ModuleViewModel : Screen
    {
        protected ModuleViewModel(IModule module)
        {
            Module = module;
        }

        public IModule Module { get; }
    }
}