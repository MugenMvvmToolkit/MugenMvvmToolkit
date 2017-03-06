#region Copyright

// ****************************************************************************
// <copyright file="ModelPropertyAttribute.cs">
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

using System;
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public sealed class ModelPropertyAttribute : Attribute
    {
        #region Fields

        private readonly string _property;

        #endregion

        #region Constructors

        public ModelPropertyAttribute([NotNull] string modelProperty)
        {
            Should.NotBeNullOrWhitespace("model", modelProperty);
            _property = modelProperty;
        }

        #endregion

        #region Properties

        [NotNull]
        public string Property => _property;

        #endregion
    }
}
