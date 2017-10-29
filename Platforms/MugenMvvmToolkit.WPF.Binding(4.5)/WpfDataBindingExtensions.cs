#region Copyright

// ****************************************************************************
// <copyright file="WpfDataBindingExtensions.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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

using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.WPF.Infrastructure;

namespace MugenMvvmToolkit.WPF.Binding
{
    public static class WpfDataBindingExtensions
    {
        #region Fields

        private static bool _initializedFromDesign;

        #endregion

        #region Methods

        public static void InitializeFromDesignContext()
        {
            BindingServiceProvider.InitializeFromDesignContext();
            if (!_initializedFromDesign)
            {
                _initializedFromDesign = true;
                var methodInfo = typeof(WpfDataBindingExtensions).GetMethodEx(nameof(InitializeFromDesignContextInternal),
                    MemberFlags.Static | MemberFlags.NonPublic | MemberFlags.Public);
                methodInfo?.Invoke(null, null);
            }
        }

        internal static void InitializeFromDesignContextInternal()
        {
            BindingServiceProvider.ValueConverter = BindingConverterExtensions.Convert;
            if (ToolkitServiceProvider.AttachedValueProvider == null)
                ToolkitServiceProvider.AttachedValueProvider = new AttachedValueProvider();
            if (ToolkitServiceProvider.ReflectionManager == null)
                ToolkitServiceProvider.ReflectionManager = new ExpressionReflectionManager();
            if (ToolkitServiceProvider.ThreadManager == null)
                ToolkitServiceProvider.ThreadManager = new SynchronousThreadManager();
        }

        #endregion
    }
}