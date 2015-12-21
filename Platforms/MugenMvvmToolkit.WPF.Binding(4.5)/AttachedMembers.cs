#region Copyright

// ****************************************************************************
// <copyright file="AttachedMembers.cs">
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
using System.Collections.Generic;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Models;
#if WINDOWSCOMMON
using UIElementEx = Windows.UI.Xaml.UIElement;
#elif XAMARIN_FORMS
using UIElementEx = Xamarin.Forms.VisualElement;
#else
using UIElementEx = System.Windows.UIElement;
#endif


#if WPF
namespace MugenMvvmToolkit.WPF.Binding
#elif WINDOWSCOMMON
namespace MugenMvvmToolkit.WinRT.Binding
#elif WINDOWS_PHONE
namespace MugenMvvmToolkit.WinPhone.Binding
#elif SILVERLIGHT
namespace MugenMvvmToolkit.Silverlight.Binding
#elif XAMARIN_FORMS
namespace MugenMvvmToolkit.Xamarin.Forms.Binding
#endif

{
    public static class AttachedMembers
    {
        #region Nested types

        public abstract class Object
        {
            #region Fields

            public static readonly BindingMemberDescriptor<object, object> DataContext;
            public static BindingMemberDescriptor<object, object> Parent;
            public static readonly BindingMemberDescriptor<object, object> CommandParameter;
            public static readonly BindingMemberDescriptor<object, IEnumerable<object>> Errors;

            #endregion

            #region Constructors

            static Object()
            {
                DataContext = new BindingMemberDescriptor<object, object>(AttachedMemberConstants.DataContext);
                Parent = new BindingMemberDescriptor<object, object>(AttachedMemberConstants.Parent);
                CommandParameter = new BindingMemberDescriptor<object, object>(AttachedMemberConstants.CommandParameter);
                Errors = new BindingMemberDescriptor<object, IEnumerable<object>>(AttachedMemberConstants.ErrorsPropertyMember);
            }

            #endregion
        }

#if XAMARIN_FORMS
        public abstract class VisualElement : Object
        {
        #region Fields

            public static readonly BindingMemberDescriptor<UIElementEx, bool> Visible;
            public static readonly BindingMemberDescriptor<UIElementEx, bool> Hidden;

        #endregion

        #region Constructors

            static VisualElement()
            {
                Visible = new BindingMemberDescriptor<UIElementEx, bool>(nameof(Visible));
                Hidden = new BindingMemberDescriptor<UIElementEx, bool>(nameof(Hidden));
            }

        #endregion
        }
#else
        public abstract class UIElement : Object
        {
            #region Fields

            public static readonly BindingMemberDescriptor<UIElementEx, bool> Visible;
            public static readonly BindingMemberDescriptor<UIElementEx, bool> Hidden;

            #endregion

            #region Constructors

            static UIElement()
            {
                Visible = new BindingMemberDescriptor<UIElementEx, bool>(nameof(Visible));
                Hidden = new BindingMemberDescriptor<UIElementEx, bool>(nameof(Hidden));
            }

            #endregion
        }
#endif
        #endregion
    }
}
