#region Copyright

// ****************************************************************************
// <copyright file="IOperationCallback.cs">
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

namespace MugenMvvmToolkit.Interfaces.Callbacks
{
    /// <summary>
    ///     Represents the operation callback.
    /// </summary>
    public interface IOperationCallback
    {
        /// <summary>
        ///     Gets a value indicating whether the <see cref="IOperationCallback" /> is serializable.
        /// </summary>
        bool IsSerializable { get; }

        /// <summary>
        ///     Invokes the callback using the specified operation result.
        /// </summary>
        void Invoke(IOperationResult result);
    }
}