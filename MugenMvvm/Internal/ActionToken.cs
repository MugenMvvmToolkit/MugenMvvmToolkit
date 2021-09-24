﻿using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using MugenMvvm.Collections;

namespace MugenMvvm.Internal
{
    [StructLayout(LayoutKind.Auto)]
    public struct ActionToken : IDisposable, IEquatable<ActionToken>
    {
        private object? _handler;
        private object? _state1;
        private object? _state2;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ActionToken(object handler, object? state, object? state2)
        {
            Should.NotBeNull(handler, nameof(handler));
            _handler = handler;
            _state1 = state;
            _state2 = state2;
        }

        public readonly bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _handler == null;
        }

        public static ActionToken NoDo
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => FromDelegate((_, _) => { });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ActionToken FromDisposable(IDisposable? disposable) => disposable == null ? default : FromDelegate((o, _) => ((IDisposable) o!).Dispose(), disposable);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ActionToken FromHandler(IHandler handler, object? state1 = null, object? state2 = null) => new(handler, state1, state2);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ActionToken FromDelegate(Action<object?, object?> handler, object? state1 = null, object? state2 = null) => new(handler, state1, state2);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ActionToken FromDelegate<T>(T state, Action<T> handler) where T : class? =>
            new(new Action<object?, object?>((a, s) => ((Action<T>) a!).Invoke((T) s!)), handler, state);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ActionToken FromDelegate(Action handler) => new(new Action<object?, object?>((a, _) => ((Action) a!).Invoke()), handler, null);

        public static ActionToken FromTokens(ItemOrIReadOnlyCollection<ActionToken> tokens)
        {
            if (tokens.HasItem)
                return tokens.Item;

            if (tokens.List != null)
            {
                return FromDelegate((o, _) =>
                {
                    foreach (var t in ItemOrIReadOnlyCollection.FromRawValue<ActionToken>(o))
                        t.Dispose();
                }, tokens.List);
            }

            return default;
        }

        public static ActionToken FromDisposable<T>(ItemOrIReadOnlyCollection<T> disposables) where T : class, IDisposable
        {
            if (disposables.HasItem)
                return FromDisposable(disposables.Item);
            if (disposables.List != null)
            {
                return FromDelegate((o, _) =>
                {
                    foreach (var t in ItemOrIReadOnlyCollection.FromRawValue<IDisposable>(o))
                        t.Dispose();
                }, disposables.List);
            }

            return default;
        }

        public void Deconstruct(out object? handler, out object? state1, out object? state2)
        {
            handler = _handler;
            state1 = _state1;
            state2 = _state2;
        }

        public void Dispose()
        {
            if (_handler == null)
                return;

            var handler = Interlocked.Exchange(ref _handler, null);
            if (handler == null)
                return;

            if (handler is IHandler h)
                h.Invoke(_state1, _state2);
            else
                ((Action<object?, object?>) handler).Invoke(_state1, _state2);
            _state1 = null;
            _state2 = null;
        }

        public readonly bool Equals(ActionToken other) => Equals(_handler, other._handler) && Equals(_state1, other._state1) && Equals(_state2, other._state2);

        public override readonly bool Equals(object? obj) => obj is ActionToken other && Equals(other);

        // ReSharper disable NonReadonlyMemberInGetHashCode
        public override readonly int GetHashCode() => HashCode.Combine(_handler, _state1, _state2);
        // ReSharper restore NonReadonlyMemberInGetHashCode

        public interface IHandler
        {
            void Invoke(object? state1, object? state2);
        }
    }
}