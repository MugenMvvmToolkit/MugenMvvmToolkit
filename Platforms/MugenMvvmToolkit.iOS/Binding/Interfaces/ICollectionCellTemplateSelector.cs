#region Copyright

// ****************************************************************************
// <copyright file="ICollectionCellTemplateSelector.cs">
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

using Foundation;
using JetBrains.Annotations;
using UIKit;

namespace MugenMvvmToolkit.iOS.Binding.Interfaces
{
    public interface ICollectionCellTemplateSelector
    {
        void Initialize([NotNull] UICollectionView container);

        NSString GetIdentifier([CanBeNull] object item, [NotNull] UICollectionView container);

        void InitializeTemplate([NotNull] UICollectionView container, UICollectionViewCell cell);
    }

    public interface ICollectionCellTemplateSelectorSupportDequeueReusableCell : ICollectionCellTemplateSelector
    {
        UICollectionViewCell DequeueReusableCell(UICollectionView collectionView, object item, NSIndexPath indexPath);
    }
}
