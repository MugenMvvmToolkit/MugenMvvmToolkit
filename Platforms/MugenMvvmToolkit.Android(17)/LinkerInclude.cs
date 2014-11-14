#region Copyright
// ****************************************************************************
// <copyright file="LinkerInclude.cs">
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
using Android.Content;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using JetBrains.Annotations;

namespace MugenMvvmToolkit
{
    internal static partial class LinkerInclude
    {
        [UsedImplicitly]
        private abstract class LinkerIncludeAdapter : AdapterView
        {
            private LinkerIncludeAdapter(Context context)
                : base(context)
            {
                RawAdapter = RawAdapter;
            }
        }

        [UsedImplicitly]
        private static void Include()
        {
            typeof(LinearLayout).GetHashCode();
            typeof(FrameLayout).GetHashCode();
            typeof(RelativeLayout).GetHashCode();
            typeof(CheckBox).GetHashCode();
            typeof(EditText).GetHashCode();

#if !API8
            var searchView = new SearchView(null);
            searchView.QueryTextChange += (sender, args) => { };
            searchView.QueryTextChange -= (sender, args) => { };
#endif
            var view = new View(null);
            view.Enabled = view.Enabled;
            view.FocusChange += (sender, args) => { };
            view.FocusChange -= (sender, args) => { };

            var button = new Button(null);
            button.Text = button.Text;
            button.Click += (sender, args) => { };
            button.Click -= (sender, args) => { };

            TextView editText = new TextView(null);
            editText.Text = editText.Text;
            editText.TextChanged += (sender, args) => { };
            editText.TextChanged -= (sender, args) => { };
            editText.Error = editText.Error;

            var prog = new ProgressBar(null);
            prog.Progress = prog.Progress;

            CompoundButton cb = null;
            cb.Checked = cb.Checked;
            cb.CheckedChange += (sender, args) => { };
            cb.CheckedChange -= (sender, args) => { };

            var rb = new RatingBar(null);
            rb.Rating = rb.Rating;
            rb.RatingBarChange += (sender, args) => { };
            rb.RatingBarChange -= (sender, args) => { };

            var dp = new DatePicker(null);
            dp.DateTime = dp.DateTime;

            var tp = new TimePicker(null);
            tp.CurrentHour = tp.CurrentHour;
            tp.CurrentMinute = tp.CurrentMinute;
        }

        [UsedImplicitly]
        private static void IncludeAdapterView<T>(AdapterView<T> adapter) where T : IAdapter
        {
            adapter.Adapter = adapter.Adapter;
        }
    }
}