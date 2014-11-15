#region Copyright
// ****************************************************************************
// <copyright file="PlatformExtensions.cs">
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
using System.Collections.Generic;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Models;
using Xamarin.Forms;

namespace MugenMvvmToolkit
{
    public static class PlatformExtensions
    {
        #region Fields

        private const string NavParamKey = "@~`NavParam";

        #endregion

        #region Methods

        internal static PlatformInfo GetPlatformInfo()
        {
            //TODO Update version.
            return new PlatformInfo(Device.OnPlatform(PlatformType.iOS, PlatformType.Android, PlatformType.WinPhone), new Version(0, 0));
        }

        public static void SetNavigationParameter([NotNull] this Page controller, object value)
        {
            Should.NotBeNull(controller, "controller");
            if (value == null)
                ServiceProvider.AttachedValueProvider.Clear(controller, NavParamKey);
            else
                ServiceProvider.AttachedValueProvider.SetValue(controller, NavParamKey, value);
        }

        public static object GetNavigationParameter([CanBeNull] this Page controller)
        {
            if (controller == null)
                return null;
            return ServiceProvider.AttachedValueProvider.GetValue<object>(controller, NavParamKey, false);
        }

        public static IList<IDataBinding> SetBindings(this BindableObject item, string bindingExpression,
            IList<object> sources = null)
        {
            return BindingServiceProvider.BindingProvider.CreateBindingsFromString(item, bindingExpression, sources);
        }

        #endregion
    }
}