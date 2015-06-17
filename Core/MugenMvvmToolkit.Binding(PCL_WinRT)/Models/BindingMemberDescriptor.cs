#region Copyright

// ****************************************************************************
// <copyright file="BindingMemberDescriptor.cs">
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
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Binding.Models
{
    /// <summary>
    ///     Represents the attached member descriptor.
    /// </summary>
    public struct BindingMemberDescriptor<TSource, TValue>
        where TSource : class
    {
        #region Fields

        /// <summary>
        ///     Gets the member path.
        /// </summary>
        public readonly string Path;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingMemberDescriptor{TSource,TValue}" /> class.
        /// </summary>
        public BindingMemberDescriptor([NotNull] string path)
        {
            Should.NotBeNull(path, "path");
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

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return Path;
        }

        #endregion
    }
}