#region Copyright

// ****************************************************************************
// <copyright file="DisplayNameAttribute.cs">
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
using System.Reflection;
using JetBrains.Annotations;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Attributes
{
    /// <summary>
    ///     Specifies the display name for a property, event, or method.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Field,
        AllowMultiple = false, Inherited = true)]
    public sealed class DisplayNameAttribute : Attribute
    {
        #region Fields

        private readonly string _displayName;
        private readonly Func<string> _resourceAccessor;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="DisplayNameAttribute" /> class.
        /// </summary>
        public DisplayNameAttribute([NotNull] string displayName)
        {
            Should.NotBeNullOrEmpty(displayName, "displayName");
            _displayName = displayName;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DisplayNameAttribute" /> class.
        /// </summary>
        public DisplayNameAttribute([NotNull] Type resourceType, [NotNull] string resourceName)
        {
            Should.NotBeNull(resourceType, "resourceType");
            Should.NotBeNullOrWhitespace(resourceName, "resourceName");
            _resourceAccessor = FindResourceAccessor(resourceName, resourceType);
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the display name.
        /// </summary>
        public string DisplayName
        {
            get
            {
                if (_resourceAccessor == null)
                    return _displayName;
                return _resourceAccessor();
            }
        }

        #endregion

        #region Methods

        private static Func<string> FindResourceAccessor(string resourceName, Type resourceType)
        {
            PropertyInfo propertyInfo = resourceType.GetPropertyEx(resourceName,
                MemberFlags.Static | MemberFlags.Public | MemberFlags.NonPublic);
            if (propertyInfo == null)
                throw ExceptionManager.ResourceNotFound(resourceName, resourceType);

            if (propertyInfo.PropertyType != typeof(string))
                throw ExceptionManager.ResourceNotString(resourceName, resourceType);

            MethodInfo methodInfo = propertyInfo.GetGetMethod(true);
            if (methodInfo == null)
                throw ExceptionManager.ResourceHasNotGetter(resourceName, resourceType);
            return (Func<string>)ServiceProvider.ReflectionManager.GetMethodDelegate(typeof(Func<string>), methodInfo);
        }

        #endregion
    }
}