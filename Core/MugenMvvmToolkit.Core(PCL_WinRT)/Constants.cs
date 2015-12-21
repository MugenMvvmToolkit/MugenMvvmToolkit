#region Copyright

// ****************************************************************************
// <copyright file="Constants.cs">
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
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

// ReSharper disable once CheckNamespace

namespace MugenMvvmToolkit.DataConstants
{
    public static class ViewModelConstants
    {
        #region Fields

        internal static readonly DataConstant<string> ViewModelTypeName;
        internal static readonly DataConstant<IDataContext> ViewModelState;
        internal static readonly DataConstant<Guid> IdParent;

        public static readonly DataConstant<Guid> Id;
        public static readonly DataConstant<object> View;
        public static readonly DataConstant<WeakReference> ParentViewModel;
        public static readonly DataConstant<IViewModel> ViewModel;
        public static readonly DataConstant<bool> StateRestored;
        public static readonly DataConstant<bool> StateNotNeeded;

        #endregion

        #region Constructors

        static ViewModelConstants()
        {
            var type = typeof(ViewModelConstants);
            Id = DataConstant.Create<Guid>(type, nameof(Id));
            IdParent = DataConstant.Create<Guid>(type, nameof(IdParent));
            View = DataConstant.Create<object>(type, nameof(View), true);
            ViewModelTypeName = DataConstant.Create<string>(type, nameof(ViewModelTypeName), true);
            ViewModelState = DataConstant.Create<IDataContext>(type, nameof(ViewModelState), true);
            ViewModel = DataConstant.Create<IViewModel>(type, nameof(ViewModel), true);
            ParentViewModel = DataConstant.Create<WeakReference>(type, nameof(ParentViewModel), true);
            StateNotNeeded = DataConstant.Create<bool>(type, nameof(StateNotNeeded));
            StateRestored = DataConstant.Create<bool>(type, nameof(StateRestored));
        }

        #endregion
    }

    public static class InitializationConstants
    {
        #region Fields

        public static readonly DataConstant<IIocContainer> IocContainer;
        public static readonly DataConstant<IViewModel> ParentViewModel;
        public static readonly DataConstant<ObservationMode> ObservationMode;
        public static readonly DataConstant<string> ViewName;
        public static readonly DataConstant<string> ViewModelBindingName;
        public static readonly DataConstant<IIocParameter[]> IocParameters;
        public static readonly DataConstant<bool> IsRestored;
        public static readonly DataConstant<Type> ViewModelType;
        public static readonly DataConstant<bool> IgnoreViewModelCache;

        #endregion

        #region Constructors

        static InitializationConstants()
        {
            var type = typeof(InitializationConstants);
            IocContainer = DataConstant.Create<IIocContainer>(type, nameof(IocContainer), true);
            ParentViewModel = DataConstant.Create<IViewModel>(type, nameof(ParentViewModel), true);
            ObservationMode = DataConstant.Create<ObservationMode>(type, nameof(ObservationMode));
            ViewModelBindingName = DataConstant.Create<string>(type, nameof(ViewModelBindingName), false);
            IocParameters = DataConstant.Create<IIocParameter[]>(type, nameof(IocParameters), true);
            IsRestored = DataConstant.Create<bool>(type, nameof(IsRestored));
            ViewName = NavigationConstants.ViewName;
            ViewModelType = DataConstant.Create<Type>(type, nameof(ViewModelType), true);
            IgnoreViewModelCache = DataConstant.Create<bool>(type, nameof(IgnoreViewModelCache));
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
        public static readonly DataConstant<bool> IsDialog;
        public static readonly DataConstant<bool> ClearBackStack;
        public static readonly DataConstant<bool> UseAnimations;

        #endregion

        #region Constructors

        static NavigationConstants()
        {
            var type = typeof(NavigationConstants);
            SuppressPageNavigation = DataConstant.Create<bool>(type, nameof(SuppressPageNavigation));
            SuppressWindowNavigation = DataConstant.Create<bool>(type, nameof(SuppressWindowNavigation));
            SuppressTabNavigation = DataConstant.Create<bool>(type, nameof(SuppressTabNavigation));
            ViewModel = DataConstant.Create<IViewModel>(type, nameof(ViewModel), true);
            ViewName = DataConstant.Create<string>(type, nameof(ViewName), false);
            IsDialog = DataConstant.Create<bool>(type, nameof(IsDialog));
            ClearBackStack = DataConstant.Create<bool>(type, nameof(ClearBackStack));
            UseAnimations = DataConstant.Create<bool>(type, nameof(UseAnimations));
        }

        #endregion
    }

    public static class OpeartionCallbackConstants
    {
        #region Fields

        public static readonly DataConstant<bool> ContinueOnCapturedContext;

        #endregion

        #region Constructors

        static OpeartionCallbackConstants()
        {
            var type = typeof(OpeartionCallbackConstants);
            ContinueOnCapturedContext = DataConstant.Create<bool>(type, nameof(ContinueOnCapturedContext));
        }

        #endregion
    }

    public static class NavigationProviderConstants
    {
        #region Fields

        public static readonly DataConstant<NavigationEventArgsBase> NavigationArgs;
        public static readonly DataConstant<NavigatingCancelEventArgsBase> NavigatingCancelArgs;
        public static readonly DataConstant<string> OperationId;
        public static readonly DataConstant<bool> BringToFront;
        public static readonly DataConstant<bool> InvalidateCache;
        public static readonly DataConstant<bool> InvalidateAllCache;

        #endregion

        #region Constructors

        static NavigationProviderConstants()
        {
            var type = typeof(NavigationProviderConstants);
            NavigationArgs = DataConstant.Create<NavigationEventArgsBase>(type, nameof(NavigationArgs), true);
            NavigatingCancelArgs = DataConstant.Create<NavigatingCancelEventArgsBase>(type, nameof(NavigatingCancelArgs), true);
            OperationId = DataConstant.Create<string>(type, nameof(OperationId), false);
            BringToFront = DataConstant.Create<bool>(type, nameof(BringToFront));
            InvalidateCache = DataConstant.Create<bool>(type, nameof(InvalidateCache));
            InvalidateAllCache = DataConstant.Create<bool>(type, nameof(InvalidateAllCache));
        }

        #endregion
    }
}