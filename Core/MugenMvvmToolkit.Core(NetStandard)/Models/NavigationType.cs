#region Copyright

// ****************************************************************************
// <copyright file="NavigationType.cs">
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

namespace MugenMvvmToolkit.Models
{
    public class NavigationType : StringConstantBase<NavigationType>
    {
        #region Fields

        public static readonly NavigationType Undefined;

        public static readonly NavigationType Tab;

        public static readonly NavigationType Window;

        public static readonly NavigationType Page;

        [NotNull]
        public readonly OperationType Operation;

        #endregion

        #region Constructors

        static NavigationType()
        {
            Tab = new NavigationType("Tab", OperationType.TabNavigation);
            Window = new NavigationType("Window", OperationType.WindowNavigation);
            Page = new NavigationType("Page", OperationType.PageNavigation);
            Undefined = new NavigationType(nameof(Undefined), null);
        }

        public NavigationType(string id, [CanBeNull] OperationType operation)
            : base(id)
        {
            Operation = operation ?? OperationType.Undefined;
        }

        #endregion        
    }
}
