using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Views;

namespace MugenMvvm.Views
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct ViewInfo : IEquatable<ViewInfo>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ViewInfo(object view)
        {
            RawView = view;
        }

        public IView? View
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => RawView as IView;
        }

        public object SourceView
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => MugenExtensions.Unwrap(RawView);
        }

        public object RawView
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Is<T>() where T : class => MugenExtensions.TryUnwrap<T>(RawView) != null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? TryGet<T>() where T : class => MugenExtensions.TryUnwrap<T>(RawView);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGet<T>([NotNullWhen(true)] out T? view) where T : class
        {
            view = MugenExtensions.TryUnwrap<T>(RawView);
            return view != null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSameView(object view) => ReferenceEquals(RawView, view) || Equals(MugenExtensions.Unwrap(RawView), MugenExtensions.Unwrap(view));

        public bool Equals(ViewInfo other) => Equals(SourceView, other.SourceView);

        public override bool Equals(object? obj) => obj is ViewInfo other && Equals(other);

        public override int GetHashCode() => SourceView == null ? 0 : SourceView.GetHashCode();

        public override string ToString() => $"source: {SourceView}, view: {View}, rawView: {RawView}";
    }
}