#region Copyright
// ****************************************************************************
// <copyright file="MetadataTypeAttribute.cs">
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
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Utils;

namespace MugenMvvmToolkit.Attributes
{
    /// <summary>
    ///     Specifies the metadata class to associate with a data model class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class MetadataTypeAttribute : Attribute
    {
        #region Fields

        private readonly string _methodName;
        private readonly Type[] _metadataClassTypes;
        private Func<IEnumerable<Type>> _accessor;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:MugenMvvmToolkit.Attributes.MetadataTypeAttribute" /> class.
        /// </summary>
        /// <param name="metadataClassTypes">The series of metadata class to reference.</param>
        public MetadataTypeAttribute([NotNull] params Type[] metadataClassTypes)
        {
            Should.NotBeNullOrEmpty(metadataClassTypes, "metadataClassTypes");
            _metadataClassTypes = metadataClassTypes;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:MugenMvvmToolkit.Attributes.MetadataTypeAttribute" /> class.
        /// </summary>
        public MetadataTypeAttribute([NotNull] string methodName)
        {
            Should.NotBeNullOrWhitespace(methodName, "methodName");
            _methodName = methodName;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:MugenMvvmToolkit.Attributes.MetadataTypeAttribute" /> class.
        /// </summary>
        public MetadataTypeAttribute([NotNull] Type metadataType, [NotNull] string methodName)
        {
            Should.NotBeNull(metadataType, "metadataType");
            Should.NotBeNullOrWhitespace(methodName, "methodName");
            _accessor = FindAccessor(methodName, metadataType);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Gets the metadata classes that is associated with a data-model partial class.
        /// </summary>
        public IEnumerable<Type> GetTypes([NotNull]Type definedType)
        {
            Should.NotBeNull(definedType, "definedType");
            if (_metadataClassTypes == null)
            {
                if (_accessor == null)
                    _accessor = FindAccessor(_methodName, definedType);
                return _accessor() ?? Enumerable.Empty<Type>();
            }
            return _metadataClassTypes;
        }

        private static Func<IEnumerable<Type>> FindAccessor(string resourceName, Type resourceType)
        {
            var methodInfo = resourceType.GetMethodEx(resourceName,
                MemberFlags.Static | MemberFlags.Public | MemberFlags.NonPublic);
            if (methodInfo == null)
                throw ExceptionManager.ResourceNotFound(resourceName, resourceType);

            if (!typeof(IEnumerable<Type>).IsAssignableFrom(methodInfo.ReturnType))
                throw ExceptionManager.ResourceHasNotGetter(resourceName, resourceType);
            return (Func<IEnumerable<Type>>)ServiceProvider.ReflectionManager.GetMethodDelegate(typeof(Func<IEnumerable<Type>>), methodInfo);
        }

        #endregion
    }
}