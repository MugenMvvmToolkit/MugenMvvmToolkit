#region Copyright

// ****************************************************************************
// <copyright file="ViewResult.cs">
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

using Android.Views;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Android.Models
{
    public struct ViewResult
    {
        #region Fields

        #endregion

        #region Constructors

        public ViewResult(View view, string bind, int? itemTemplateId, int? dropDownItemTemplateId, int? contentTemplateId, int? menuTemplateId, int? popupMenuTemplateId,
            string popupMenuEvent, string placementTargetPath, IDataContext dataContext = null)
        {
            View = view;
            Bind = bind;
            ItemTemplateId = itemTemplateId;
            DropDownItemTemplateId = dropDownItemTemplateId;
            ContentTemplateId = contentTemplateId;
            MenuTemplateId = menuTemplateId;
            PopupMenuTemplateId = popupMenuTemplateId;
            PopupMenuEvent = popupMenuEvent;
            PlacementTargetPath = placementTargetPath;
            DataContext = dataContext;
        }

        #endregion

        #region Properties

        public bool IsEmpty => View == null;

        public View View { get; }

        public string Bind { get; }

        public int? ItemTemplateId { get; }

        public int? DropDownItemTemplateId { get; }

        public int? ContentTemplateId { get; }

        public int? MenuTemplateId { get; }

        public int? PopupMenuTemplateId { get; }

        public string PopupMenuEvent { get; }

        public string PlacementTargetPath { get; }

        [CanBeNull]
        public IDataContext DataContext { get; }

        #endregion
    }
}