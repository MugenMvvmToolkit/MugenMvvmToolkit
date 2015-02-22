#region Copyright

// ****************************************************************************
// <copyright file="PlatformExtensions.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using System;
using System.Windows;
using JetBrains.Annotations;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Modules;
using NavigationMode = MugenMvvmToolkit.Models.NavigationMode;

namespace MugenMvvmToolkit
{
    public static class PlatformExtensions
    {
        #region Fields

        private static Func<Type, DataTemplate> _defaultViewModelTemplateFactory;

        #endregion

        #region Constructors

        static PlatformExtensions()
        {
            _defaultViewModelTemplateFactory = ViewModelDataTemplateModule.DefaultTemplateProvider;
        }

        #endregion

        #region Properties

        [NotNull]
        public static Func<Type, DataTemplate> DefaultViewModelTemplateFactory
        {
            get { return _defaultViewModelTemplateFactory; }
            set
            {
                Should.PropertyNotBeNull(value);
                _defaultViewModelTemplateFactory = value;
            }
        }

        #endregion

        #region Methods

        internal static void AsEventHandler<TArg>(this Action action, object sender, TArg arg)
        {
            action();
        }

        internal static PlatformInfo GetPlatformInfo()
        {
#if WPF
            return new PlatformInfo(PlatformType.WPF, Environment.Version);
#endif
        }

        internal static NavigationMode ToNavigationMode(this System.Windows.Navigation.NavigationMode mode)
        {
            switch (mode)
            {
                case System.Windows.Navigation.NavigationMode.New:
                    return NavigationMode.New;
                case System.Windows.Navigation.NavigationMode.Back:
                    return NavigationMode.Back;
                case System.Windows.Navigation.NavigationMode.Forward:
                    return NavigationMode.Forward;
                case System.Windows.Navigation.NavigationMode.Refresh:
                    return NavigationMode.Refresh;
#if WINDOWS_PHONE8
                case System.Windows.Navigation.NavigationMode.Reset:
                    return NavigationMode.Reset;
#endif
                default:
                    return NavigationMode.Undefined;
            }
        }

        #endregion
    }
}