﻿#region Greenshot GNU General Public License

// Greenshot - a free and open source screenshot tool
// Copyright (C) 2007-2018 Thomas Braun, Jens Klingen, Robin Krom
// 
// For more information see: http://getgreenshot.org/
// The Greenshot project is hosted on GitHub https://github.com/greenshot/greenshot
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 1 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Reactive.Disposables;
using Autofac.Features.OwnedInstances;
using Caliburn.Micro;
using Dapplo.CaliburnMicro.Configuration;
using Dapplo.CaliburnMicro.Extensions;
using Dapplo.HttpExtensions.OAuth;
using Greenshot.Addon.Imgur.Configuration;
using Greenshot.Addons.Core.Enums;
using Greenshot.Addons.ViewModels;

namespace Greenshot.Addon.Imgur.ViewModels
{
    /// <summary>
    /// The imgure config VM
    /// </summary>
    public sealed class ImgurConfigViewModel : SimpleConfigScreen
    {
        /// <summary>
        ///     Here all disposables are registered, so we can clean the up
        /// </summary>
        private CompositeDisposable _disposables;

        public IImgurConfiguration ImgurConfiguration { get; }

        public IImgurLanguage ImgurLanguage { get; }

        public IWindowManager WindowManager { get; }

        public Func<Owned<ImgurHistoryViewModel>> ImgurHistoryViewModelFactory { get;}

        public FileConfigPartViewModel FileConfigPartViewModel { get; }

        public ImgurConfigViewModel(
            IImgurConfiguration imgurConfiguration,
            IImgurLanguage imgurLanguage ,
            IWindowManager windowManager,
            Func<Owned<ImgurHistoryViewModel>> imgurHistoryViewModelFactory,
            FileConfigPartViewModel fileConfigPartViewModel
            )
        {
            ImgurConfiguration = imgurConfiguration;
            ImgurLanguage = imgurLanguage;
            WindowManager = windowManager;
            ImgurHistoryViewModelFactory = imgurHistoryViewModelFactory;
            FileConfigPartViewModel = fileConfigPartViewModel;
        }
        public override void Initialize(IConfig config)
        {
            // Make sure the destination settings are shown
            FileConfigPartViewModel.DestinationFileConfiguration = ImgurConfiguration;

            // Prepare disposables
            _disposables?.Dispose();

            // Place this under the Ui parent
            ParentId = nameof(ConfigIds.Destinations);

            // Make sure Commit/Rollback is called on the IUiConfiguration
            config.Register(ImgurConfiguration);

            // automatically update the DisplayName
            _disposables = new CompositeDisposable
            {
                ImgurLanguage.CreateDisplayNameBinding(this, nameof(IImgurLanguage.SettingsTitle))
            };

            base.Initialize(config);
        }

        protected override void OnDeactivate(bool close)
        {
            _disposables.Dispose();
            base.OnDeactivate(close);
        }

        public void ShowHistory()
        {
            using (var imgurHistoryViewModel = ImgurHistoryViewModelFactory())
            {
                WindowManager.ShowDialog(imgurHistoryViewModel.Value);
            }
        }

        public bool CanResetCredentials => !ImgurConfiguration.AnonymousAccess && ImgurConfiguration.HasToken();

        public void ResetCredentials()
        {
            ImgurConfiguration.ResetToken();
            NotifyOfPropertyChange(nameof(CanResetCredentials));
        }
    }
}
