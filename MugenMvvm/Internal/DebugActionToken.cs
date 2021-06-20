using System;
using System.Collections.Generic;
using System.Threading;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Internal
{
    public sealed class DebugActionToken : IDisposable, IHasTarget<object>
    {
        private static readonly List<WeakReference> Tokens = new(16);
        private readonly WeakReference _weakReference;
        private readonly int _threadId;
        private readonly string? _stacktrace;
        private ActionToken _token;
        private int _state;

        private DebugActionToken(object target, ActionToken token, bool includeStackTrace)
        {
            Should.NotBeNull(target, nameof(target));
            _token = token;
            Target = target;
            _threadId = Environment.CurrentManagedThreadId;
            if (includeStackTrace)
                _stacktrace = Environment.StackTrace;
            _weakReference = new WeakReference(this);
            lock (Tokens)
            {
                Tokens.Add(_weakReference);
            }
        }

        ~DebugActionToken()
        {
            ExceptionManager.ThrowActionTokenDisposeNotCalled(_threadId, Target, _stacktrace);
        }

        public object Target { get; }

        public static ActionToken Wrap(object target, ActionToken actionToken, bool includeStackTrace) =>
            ActionToken.FromDisposable(new DebugActionToken(target, actionToken, includeStackTrace));

        public static ItemOrIReadOnlyList<DebugActionToken> GetTokens()
        {
            var tokens = new ItemOrListEditor<DebugActionToken>();
            lock (Tokens)
            {
                for (var i = 0; i < Tokens.Count; i++)
                {
                    var token = Tokens[i];
                    if (token.Target is DebugActionToken t)
                        tokens.Add(t);
                    else
                    {
                        Tokens.RemoveAt(i);
                        --i;
                    }
                }

                return tokens;
            }
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _state, 1) != 0)
                ExceptionManager.ThrowObjectDisposed(this);

            lock (Tokens)
            {
                Tokens.Remove(_weakReference);
            }

            _token.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}