#region Copyright

// ****************************************************************************
// <copyright file="BootstrapperAttribute.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Android.Infrastructure;

namespace MugenMvvmToolkit.Android.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class BootstrapperAttribute : Attribute
    {
        #region Fields

        private readonly Type _bootstrapperType;
        private readonly int _priority;

        #endregion

        #region Constructors

        public BootstrapperAttribute([NotNull] Type bootstrapperType, int priority = 0)
        {
            Should.BeOfType<AndroidBootstrapperBase>(bootstrapperType, "bootstrapperType");
            _bootstrapperType = bootstrapperType;
            _priority = priority;
        }

        #endregion

        #region Properties

        [NotNull]
        public Type BootstrapperType => _bootstrapperType;

        public int Priority => _priority;

        #endregion
    }
}
