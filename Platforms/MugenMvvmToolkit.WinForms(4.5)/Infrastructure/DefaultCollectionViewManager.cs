#region Copyright
// ****************************************************************************
// <copyright file="DefaultCollectionViewManager.cs">
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
using System.Windows.Forms;
using MugenMvvmToolkit.Interfaces;

namespace MugenMvvmToolkit.Infrastructure
{
    internal sealed class DefaultCollectionViewManager : ICollectionViewManager
    {
        #region Fields

        public static readonly DefaultCollectionViewManager Instance = new DefaultCollectionViewManager();

        #endregion

        #region Constructors

        private DefaultCollectionViewManager()
        {
        }

        #endregion

        #region Implementation of ICollectionViewManager

        /// <summary>
        ///     Inserts an item to the specified index.
        /// </summary>
        public void Insert(object view, int index, object item)
        {
            var tabControl = view as TabControl;
            if (tabControl != null)
            {
                tabControl.TabPages.Insert(index, (TabPage)item);
                return;
            }
            var tableLayoutPanel = view as TableLayoutPanel;
            if (tableLayoutPanel != null)
            {
                if (tableLayoutPanel.RowCount <= index)
                    tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                var itemToAdd = (Control)item;
                tableLayoutPanel.Controls.Add(itemToAdd, 0, index);
                tableLayoutPanel.Controls.SetChildIndex(itemToAdd, index);
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

        /// <summary>
        ///     Removes an item.
        /// </summary>
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

        /// <summary>
        ///     Removes all items.
        /// </summary>
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
                tableLayoutPanel.RowStyles.Clear();
                tableLayoutPanel.Controls.Clear();
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

        private void TraceNotSupported(object view)
        {
            Tracer.Warn("The view '{0}' is not supported by '{1}'", view, this);
        }

        #endregion

    }
}