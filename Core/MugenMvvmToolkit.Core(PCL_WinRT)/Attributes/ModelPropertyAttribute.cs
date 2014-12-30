#region Copyright

// ****************************************************************************
// <copyright file="ModelPropertyAttribute.cs">
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

namespace MugenMvvmToolkit.Attributes
{
    /// <summary>
    ///     Attribute to link a property in a model to a view model.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public sealed class ModelPropertyAttribute : Attribute
    {
        #region Fields

        private readonly string _property;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ModelPropertyAttribute" /> class.
        /// </summary>
        public ModelPropertyAttribute([NotNull] string modelProperty)
        {
            Should.NotBeNullOrWhitespace("model", modelProperty);
            _property = modelProperty;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the property name that holds the model object.
        /// </summary>
        /// <value>The model property name.</value>
        [NotNull]
        public string Property
        {
            get { return _property; }
        }

        #endregion
    }
}