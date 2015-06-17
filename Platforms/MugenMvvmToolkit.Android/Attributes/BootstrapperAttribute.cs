#region Copyright

// ****************************************************************************
// <copyright file="BootstrapperAttribute.cs">
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

        /// <summary>
        ///     Initializes a new instance of the <see cref="BootstrapperAttribute" /> class.
        /// </summary>
        public BootstrapperAttribute([NotNull] Type bootstrapperType, int priority = 0)
        {
            Should.BeOfType<AndroidBootstrapperBase>(bootstrapperType, "bootstrapperType");
            _bootstrapperType = bootstrapperType;
            _priority = priority;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the type of bootstrapper.
        /// </summary>
        [NotNull]
        public Type BootstrapperType
        {
            get { return _bootstrapperType; }
        }

        /// <summary>
        ///     Gets the priority.
        /// </summary>
        public int Priority
        {
            get { return _priority; }
        }

        #endregion
    }
}