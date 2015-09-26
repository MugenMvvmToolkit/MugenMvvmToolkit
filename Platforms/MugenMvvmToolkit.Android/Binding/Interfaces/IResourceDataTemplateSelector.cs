#region Copyright

// ****************************************************************************
// <copyright file="IResourceDataTemplateSelector.cs">
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
using MugenMvvmToolkit.Binding.Interfaces;

namespace MugenMvvmToolkit.Android.Binding.Interfaces
{
    public interface IResourceDataTemplateSelector : IDataTemplateSelector
    {
        int TemplateTypeCount { get; }

        new int SelectTemplate([CanBeNull] object item, [NotNull] object container);
    }
}
