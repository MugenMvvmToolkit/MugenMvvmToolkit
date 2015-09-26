#region Copyright

// ****************************************************************************
// <copyright file="MetadataTypeAttribute.cs">
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
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class MetadataTypeAttribute : Attribute
    {
        #region Fields

        private readonly string _methodName;
        private readonly Type[] _metadataClassTypes;
        private Func<IEnumerable<Type>> _accessor;

        #endregion

        #region Constructors

        public MetadataTypeAttribute([NotNull] params Type[] metadataClassTypes)
        {
            Should.NotBeNullOrEmpty(metadataClassTypes, "metadataClassTypes");
            _metadataClassTypes = metadataClassTypes;
        }

        public MetadataTypeAttribute([NotNull] string methodName)
        {
            Should.NotBeNullOrWhitespace(methodName, "methodName");
            _methodName = methodName;
        }

        public MetadataTypeAttribute([NotNull] Type metadataType, [NotNull] string methodName)
        {
            Should.NotBeNull(metadataType, "metadataType");
            Should.NotBeNullOrWhitespace(methodName, "methodName");
            _accessor = FindAccessor(methodName, metadataType);
        }

        #endregion

        #region Methods

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
