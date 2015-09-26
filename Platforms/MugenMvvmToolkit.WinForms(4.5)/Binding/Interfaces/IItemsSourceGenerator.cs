#region Copyright

// ****************************************************************************
// <copyright file="IItemsSourceGenerator.cs">
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

using System.Collections;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;

#if ANDROID
namespace MugenMvvmToolkit.Android.Binding.Interfaces
#elif TOUCH
namespace MugenMvvmToolkit.iOS.Binding.Interfaces
#elif WINFORMS
namespace MugenMvvmToolkit.WinForms.Binding.Interfaces
#elif SILVERLIGHT
namespace MugenMvvmToolkit.Silverlight.Binding.Interfaces
#endif
{
    public interface IItemsSourceGenerator
    {
        [CanBeNull]
        IEnumerable ItemsSource { get; }

        void SetItemsSource([CanBeNull] IEnumerable itemsSource, IDataContext context = null);

        void Reset();
    }

#if ANDROID
    public interface IItemsSourceGeneratorEx : IItemsSourceGenerator
    {
        object SelectedItem { get; set; }
    }
#endif

}
