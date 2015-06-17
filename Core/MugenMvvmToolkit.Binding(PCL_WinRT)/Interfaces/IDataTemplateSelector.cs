#region Copyright

// ****************************************************************************
// <copyright file="IDataTemplateSelector.cs">
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

namespace MugenMvvmToolkit.Binding.Interfaces
{
    /// <summary>
    ///     DataTemplateSelector allows the app writer to provide custom template selection logic.
    /// </summary>
    public interface IDataTemplateSelector
    {
        /// <summary>
        ///     Returns an app specific template.
        /// </summary>
        /// <param name="item">The data content</param>
        /// <param name="container">The element to which the template will be applied</param>
        /// <returns>An app-specific template to apply, or null.</returns>
        object SelectTemplate([CanBeNull] object item, [NotNull] object container);
    }
}