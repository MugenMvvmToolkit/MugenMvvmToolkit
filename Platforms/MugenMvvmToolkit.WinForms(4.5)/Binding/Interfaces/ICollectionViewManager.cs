#region Copyright

// ****************************************************************************
// <copyright file="ICollectionViewManager.cs">
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

#if WINFORMS
namespace MugenMvvmToolkit.WinForms.Binding.Interfaces
#elif ANDROID
namespace MugenMvvmToolkit.Android.Binding.Interfaces
#elif TOUCH
namespace MugenMvvmToolkit.iOS.Binding.Interfaces
#endif
{
    public interface ICollectionViewManager
    {
        void Insert([NotNull] object view, int index, object viewItem);

        void RemoveAt([NotNull] object view, int index);

        void Clear([NotNull] object view);
    }
}
