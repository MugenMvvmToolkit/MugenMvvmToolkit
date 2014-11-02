#region Copyright
// ****************************************************************************
// <copyright file="Constants.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Models;

// ReSharper disable once CheckNamespace
namespace MugenMvvmToolkit.DataConstants
{
    public static class ViewModelConstants
    {
        #region Fields

        internal static readonly DataConstant<string> ViewModelTypeName;

        internal static readonly DataConstant<IDataContext> ViewModelState;

        public static readonly DataConstant<IView> View;

        public static readonly DataConstant<object> StateManager;

        public static readonly DataConstant<WeakReference> ParentViewModel;

        public static readonly DataConstant<IViewModel> ViewModel;

        public static readonly DataConstant<bool> StateNotNeeded;

        public static readonly DataConstant<bool> StateRestored;

        #endregion

        #region Constructors

        static ViewModelConstants()
        {
            View = DataConstant.Create(() => View, true);
            ViewModelTypeName = DataConstant.Create(() => ViewModelTypeName, true);
            ViewModelState = DataConstant.Create(() => ViewModelState, true);
            StateManager = DataConstant.Create(() => StateManager, true);
            ViewModel = DataConstant.Create(() => ViewModel, true);
            ParentViewModel = DataConstant.Create(() => ParentViewModel, true);
            StateNotNeeded = DataConstant.Create(() => StateNotNeeded);
            StateRestored = DataConstant.Create(() => StateRestored);
        }

        #endregion
    }

    public static class InitializationConstants
    {
        #region Fields

        public static readonly DataConstant<IocContainerCreationMode> IocContainerCreationMode;

        public static readonly DataConstant<IIocContainer> IocContainer;

        public static readonly DataConstant<IViewModel> ParentViewModel;

        public static readonly DataConstant<ObservationMode> ObservationMode;

        public static readonly DataConstant<string> ViewName;

        public static readonly DataConstant<string> ViewModelBindingName;

        public static readonly DataConstant<IIocParameter[]> IocParameters;

        public static readonly DataConstant<bool> IsRestored;

        public static readonly DataConstant<Type> ViewModelType;

        public static readonly DataConstant<bool> IgnoreRestoredViewModelCache;

        #endregion

        #region Constructors

        static InitializationConstants()
        {
            IocContainerCreationMode = DataConstant.Create(() => IocContainerCreationMode);
            IocContainer = DataConstant.Create(() => IocContainer, true);
            ParentViewModel = DataConstant.Create(() => ParentViewModel, true);
            ObservationMode = DataConstant.Create(() => ObservationMode);
            ViewModelBindingName = DataConstant.Create(() => ViewModelBindingName, false);
            IocParameters = DataConstant.Create(() => IocParameters, true);
            IsRestored = DataConstant.Create(() => IsRestored);
            ViewName = NavigationConstants.ViewName;
            ViewModelType = DataConstant.Create(() => ViewModelType, true);
            IgnoreRestoredViewModelCache = DataConstant.Create(() => IgnoreRestoredViewModelCache);
        }

        #endregion
    }

    public static class NavigationConstants
    {
        #region Fields

        public static readonly DataConstant<bool> SuppressTabNavigation;

        public static readonly DataConstant<bool> SuppressWindowNavigation;

        public static readonly DataConstant<bool> SuppressPageNavigation;

        public static readonly DataConstant<IViewModel> ViewModel;

        public static readonly DataConstant<string> ViewName;

        public static readonly DataConstant<IDataContext> Parameters;

        public static readonly DataConstant<bool> IsDialog;

        #endregion

        #region Constructors

        static NavigationConstants()
        {
            SuppressPageNavigation = DataConstant.Create(() => SuppressPageNavigation);
            SuppressWindowNavigation = DataConstant.Create(() => SuppressWindowNavigation);
            SuppressTabNavigation = DataConstant.Create(() => SuppressTabNavigation);
            ViewModel = DataConstant.Create(() => ViewModel, true);
            ViewName = DataConstant.Create(() => ViewName, false);
            Parameters = DataConstant.Create(() => Parameters, true);
            IsDialog = DataConstant.Create(() => IsDialog);
        }

        #endregion
    }
}