#region Copyright

// ****************************************************************************
// <copyright file="AttachedMemberConstants.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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

namespace MugenMvvmToolkit.Binding
{
    /// <summary>
    ///     Represents the class that contains the attached member names
    /// </summary>
    public static class AttachedMemberConstants
    {
        /// <summary>
        ///     Gets the data context member name.
        /// </summary>
        public const string DataContext = "DataContext";

        /// <summary>
        ///     Gets the command parameter member name.
        /// </summary>
        public const string CommandParameter = "CommandParameter";

        /// <summary>
        ///     Gets the name of member that will be used by IsEnabled property.
        /// </summary>
        public const string Enabled = "Enabled";

        /// <summary>
        ///     Gets the name of member that will be used by IsFocused property.
        /// </summary>
        public const string Focused = "#FocusedMemberName";

        /// <summary>
        ///     Gets the name of member that will be used by GetParentMember method.
        /// </summary>
        public const string Parent = "Parent";

        /// <summary>
        ///     Gets the name of member that will be used by GetRootMember method.
        /// </summary>
        public const string RootElement = "#RootElement";

        /// <summary>
        ///     Gets the name of member that will be used by FindByName method.
        /// </summary>
        public const string FindByNameMethod = "#FindByName";

        /// <summary>
        ///     Gets the name of member that will be used by SetErrors method.
        /// </summary>
        public const string ErrorsPropertyMember = "Errors";

        /// <summary>
        ///     Gets the content member name.
        /// </summary>
        public const string Content = "Content";

        /// <summary>
        ///     Gets the selected item member name.
        /// </summary>
        public const string SelectedItem = "SelectedItem";

        /// <summary>
        ///     Gets the items source member name.
        /// </summary>
        public const string ItemsSource = "ItemsSource";

        /// <summary>
        ///     Gets the content template member name.
        /// </summary>
        public const string ContentTemplate = "ContentTemplate";

        /// <summary>
        ///     Gets the item template member name.
        /// </summary>
        public const string ItemTemplate = "ItemTemplate";

        /// <summary>
        ///     Gets the content template selector name.
        /// </summary>
        public const string ContentTemplateSelector = "ContentTemplateSelector";

        /// <summary>
        ///     Gets the item template selector name.
        /// </summary>
        public const string ItemTemplateSelector = "ItemTemplateSelector";
    }
}