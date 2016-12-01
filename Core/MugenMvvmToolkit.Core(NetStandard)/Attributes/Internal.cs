#region Copyright

// ****************************************************************************
// <copyright file="Internal.cs">
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

#if !HASSERIALIZABLE
// ReSharper disable once CheckNamespace
namespace System
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Delegate,
        Inherited = false)]
    internal sealed class SerializableAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false)]
    internal sealed class NonSerializedAttribute : Attribute
    {
    }
}
#endif

namespace MugenMvvmToolkit.Attributes
{
    [AttributeUsage(
         AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method |
         AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Delegate)]
    internal sealed class PreserveAttribute : Attribute
    {
        #region Fields

        public bool AllMembers;

        public bool Conditional;

        #endregion
    }

    [AttributeUsage(AttributeTargets.Assembly)]
    internal class LinkerSafeAttribute : Attribute
    {
    }
}

