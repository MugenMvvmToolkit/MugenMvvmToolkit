#region Copyright

// ****************************************************************************
// <copyright file="MetadataTypeAttribute.cs">
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

// ReSharper disable once CheckNamespace

namespace System.ComponentModel.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class MetadataTypeAttribute : Attribute
    {
        #region Constructors

        public MetadataTypeAttribute(Type metadataClassType)
        {
            MetadataClassType = metadataClassType;
        }

        #endregion

        #region Properties

        public Type MetadataClassType { get; private set; }

        #endregion
    }
}