﻿using Artemis.Core.Models.Profile;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Services.Interfaces;

namespace Artemis.UI.Screens.Module.ProfileEditor.ProfileTree.TreeItem
{
    public class FolderViewModel : TreeItemViewModel
    {
        // I hate this about DI, oh well
        public FolderViewModel(ProfileElement folder,
            IProfileEditorService profileEditorService,
            IDialogService dialogService,
            IFolderViewModelFactory folderViewModelFactory,
            ILayerViewModelFactory layerViewModelFactory) :
            base(null, folder, profileEditorService, dialogService, folderViewModelFactory, layerViewModelFactory)
        {
        }

        public FolderViewModel(TreeItemViewModel parent,
            ProfileElement folder,
            IProfileEditorService profileEditorService,
            IDialogService dialogService,
            IFolderViewModelFactory folderViewModelFactory,
            ILayerViewModelFactory layerViewModelFactory) :
            base(parent, folder, profileEditorService, dialogService, folderViewModelFactory, layerViewModelFactory)
        {
        }

        public override bool SupportsChildren => true;
    }
}