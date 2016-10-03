#region Copyright

// ****************************************************************************
// <copyright file="BindingMemberDescriptor.cs">
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

using JetBrains.Annotations;

namespace MugenMvvmToolkit.Binding.Models
{
    public struct BindingMemberDescriptor<TSource, TValue>
        where TSource : class
    {
        #region Fields

        public readonly string Path;

        #endregion

        #region Constructors

        public BindingMemberDescriptor([NotNull] string path)
        {
            Should.NotBeNull(path, nameof(path));
            Path = path;
        }

        #endregion

        #region Methods

        [Pure]
        public BindingMemberDescriptor<TNewSource, TValue> Override<TNewSource>()
            where TNewSource : class
        {
            return new BindingMemberDescriptor<TNewSource, TValue>(Path);
        }

        [Pure]
        public BindingMemberDescriptor<TSource, TNewType> Cast<TNewType>()
        {
            return new BindingMemberDescriptor<TSource, TNewType>(Path);
        }

        public static implicit operator BindingMemberDescriptor<TSource, TValue>(string path)
        {
            return new BindingMemberDescriptor<TSource, TValue>(path);
        }

        public static implicit operator string(BindingMemberDescriptor<TSource, TValue> descriptor)
        {
            return descriptor.Path;
        }

        #endregion

        #region Overrides of Object

        public override string ToString()
        {
            return Path;
        }

        #endregion
    }
}
