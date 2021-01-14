using Android.Runtime;
using MugenMvvm.Android.Native.Views.Activities;
using MugenMvvm.Android.Native.Views.Fragments;

namespace MugenMvvm.Android
{
    [Preserve(AllMembers = true)]
    internal static class LinkerInclude
    {
        public static void Include()
        {
            new ActivityWrapper(null!);
            new FragmentWrapper(null!);
            new DialogFragmentWrapper(null!);
        }
    }
}