#region Copyright

// ****************************************************************************
// <copyright file="ViewFactoryConstants.cs">
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

using System.Collections.Generic;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Android.DataConstants
{
    public static class ViewFactoryConstants
    {
        #region Fields

        public static readonly DataConstant<IList<string>> Bindings;

        public static readonly DataConstant<int> ItemTemplateId;

        public static readonly DataConstant<int> DropDownItemTemplateId;

        public static readonly DataConstant<int> ContentTemplateId;

        public static readonly DataConstant<int> MenuTemplateId;

        public static readonly DataConstant<int> PopupMenuTemplateId;

        public static readonly DataConstant<string> PopupMenuEvent;

        public static readonly DataConstant<string> PlacementTargetPath;

        #endregion

        #region Constructors

        static ViewFactoryConstants()
        {
            var type = typeof(ViewFactoryConstants);
            Bindings = DataConstant.Create<IList<string>>(type, nameof(Bindings), false);
            ItemTemplateId = DataConstant.Create<int>(type, nameof(ItemTemplateId));
            DropDownItemTemplateId = DataConstant.Create<int>(type, nameof(DropDownItemTemplateId));
            ContentTemplateId = DataConstant.Create<int>(type, nameof(ContentTemplateId));
            MenuTemplateId = DataConstant.Create<int>(type, nameof(MenuTemplateId));
            PopupMenuTemplateId = DataConstant.Create<int>(type, nameof(PopupMenuTemplateId));
            PopupMenuEvent = DataConstant.Create<string>(type, nameof(PopupMenuEvent), true);
            PlacementTargetPath = DataConstant.Create<string>(type, nameof(PlacementTargetPath), true);
        }

        #endregion
    }
}
