#region Copyright

// ****************************************************************************
// <copyright file="BindingEditorUITypeEditor.cs">
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

using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace MugenMvvmToolkit.WinForms.Binding.UiDesigner
{
    internal sealed class BindingEditorUITypeEditor : UITypeEditor
    {
        #region Overrides of UITypeEditor

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var editorService = provider.GetService<IWindowsFormsEditorService>();
            if (value != null && editorService != null)
            {
                using (var editor = new BindingEditorView(value.ToString()))
                {
                    if (editorService.ShowDialog(editor) == DialogResult.OK)
                        return editor.BindingText;
                }
            }
            return value;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        #endregion
    }
}
