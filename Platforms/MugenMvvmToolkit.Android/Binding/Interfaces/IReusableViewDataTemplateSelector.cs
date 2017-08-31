#region Copyright

// ****************************************************************************
// <copyright file="IReusableViewDataTemplateSelector.cs">
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

using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces;

namespace MugenMvvmToolkit.Android.Binding.Interfaces
{
    public interface IReusableViewDataTemplateSelector : IDataTemplateSelector
    {
        object SelectTemplate(object item, [NotNull] object container, object convertView);
    }
}