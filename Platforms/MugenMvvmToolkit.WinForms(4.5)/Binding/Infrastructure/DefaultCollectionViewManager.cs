#region Copyright

// ****************************************************************************
// <copyright file="DefaultCollectionViewManager.cs">
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

using System.Windows.Forms;
using MugenMvvmToolkit.WinForms.Binding.Interfaces;

namespace MugenMvvmToolkit.WinForms.Binding.Infrastructure
{
    internal sealed class DefaultCollectionViewManager : ICollectionViewManager
    {
        #region Fields

        public static readonly DefaultCollectionViewManager Instance;

        #endregion

        #region Constructors

        static DefaultCollectionViewManager()
        {
            Instance = new DefaultCollectionViewManager();
        }

        private DefaultCollectionViewManager()
        {
        }

        #endregion

        #region Implementation of ICollectionViewManager

        public void Insert(object view, int index, object item)
        {
            var tabControl = view as TabControl;
            if (tabControl != null)
            {
                tabControl.TabPages.Insert(index, (TabPage)item);
                return;
            }

            var control = view as Control;
            if (control == null)
                TraceNotSupported(view);
            else
            {
                var itemToAdd = (Control)item;
                control.Controls.Add(itemToAdd);
                control.Controls.SetChildIndex(itemToAdd, index);
            }
        }

        public void RemoveAt(object view, int index)
        {
            var tabControl = view as TabControl;
            if (tabControl != null)
            {
                tabControl.TabPages.RemoveAt(index);
                return;
            }

            var control = view as Control;
            if (control == null)
                TraceNotSupported(view);
            else
                control.Controls.RemoveAt(index);
        }

        public void Clear(object view)
        {
            var tabControl = view as TabControl;
            if (tabControl != null)
            {
                tabControl.TabPages.Clear();
                return;
            }
            var tableLayoutPanel = view as TableLayoutPanel;
            if (tableLayoutPanel != null)
            {
                tableLayoutPanel.Controls.Clear();
                tableLayoutPanel.RowStyles.Clear();
                tableLayoutPanel.ColumnStyles.Clear();
                tableLayoutPanel.ColumnCount = 1;
                tableLayoutPanel.RowCount = 0;
                return;
            }

            var control = view as Control;
            if (control == null)
                TraceNotSupported(view);
            else
                control.Controls.Clear();
        }

        #endregion

        #region Methods

        private static void TraceNotSupported(object view)
        {
            Tracer.Warn("The view '{0}' is not supported by '{1}'", view, typeof(DefaultCollectionViewManager));
        }

        #endregion
    }
}
