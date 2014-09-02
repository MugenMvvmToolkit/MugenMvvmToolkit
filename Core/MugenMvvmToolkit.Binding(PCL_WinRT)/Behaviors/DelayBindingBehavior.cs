#region Copyright
// ****************************************************************************
// <copyright file="DelayBindingBehavior.cs">
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
using System;
using System.Threading;
#if PCL_WINRT
using System.Threading.Tasks;
#endif
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Accessors;
using MugenMvvmToolkit.Binding.Models.EventArg;

namespace MugenMvvmToolkit.Binding.Behaviors
{
    /// <summary>
    ///     Represents the binding behavior that allows to wait before updating the binding source after the value on the
    ///     target changes.
    /// </summary>
    public sealed class DelayBindingBehavior : BindingBehaviorBase
    {
        #region Nested types

#if PCL_WINRT
        private sealed class Timer
        {
        #region Fields

            private readonly Action<object> _callback;
            private readonly object _state;
            private CancellationTokenSource _currentTokenSource;

            #endregion

        #region Constructors

            public Timer(Action<object> callback, object state, int dueTime, int period)
            {
                _callback = callback;
                _state = state;
                Change(dueTime, period);
            }

            #endregion

        #region Methods

            public void Change(int dueTime, int period)
            {
                CancellationToken token;
                //NOTE in this case is a normal lock.
                lock (this)
                {
                    if (_currentTokenSource != null)
                    {
                        _currentTokenSource.Cancel();
                        _currentTokenSource = null;
                    }
                    if (dueTime == int.MaxValue)
                        return;
                    _currentTokenSource = new CancellationTokenSource();
                    token = _currentTokenSource.Token;
                }
                Task.Delay(dueTime, token).ContinueWith((t, s) =>
                {
                    var timer = (Timer)s;
                    timer._callback(timer._state);
                }, this, token,
                    TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion,
                    TaskScheduler.Default);
            }

            public void Dispose()
            {
                lock (this)
                {
                    if (_currentTokenSource == null)
                        return;
                    _currentTokenSource.Cancel();
                    _currentTokenSource = null;
                }
            }

            #endregion
        }
#endif


        #endregion

        #region Fields

        /// <summary>
        ///     Gets the id of behavior.
        /// </summary>
        public static readonly Guid IdDelayBindingBehavior;

#if PCL_WINRT
        private static readonly Action<object> CallbackInternalDelegate;
#else
        private static readonly TimerCallback CallbackInternalDelegate;
#endif

        private static readonly SendOrPostCallback CallbackDelegate;

        private SynchronizationContext _context;
        private readonly int _delay;

        private bool _isUpdating;
        private Timer _timer;

        #endregion

        #region Constructors

        static DelayBindingBehavior()
        {
            IdDelayBindingBehavior = new Guid("5A471157-5E3B-4145-ACC4-9FEA8D1B3A99");
            CallbackInternalDelegate = CallbackInternal;
            CallbackDelegate = Callback;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DelayBindingBehavior" /> class.
        /// </summary>
        /// <param name="delay">
        ///     The amount of time, in milliseconds, to wait before updating the binding source after the value on
        ///     the target changes.
        /// </param>
        public DelayBindingBehavior(uint delay)
        {
            _delay = (int)delay;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the amount of time, in milliseconds, to wait before updating the binding source after the value on the target
        ///     changes.
        /// </summary>
        public int Delay
        {
            get { return _delay; }
        }

        #endregion

        #region Overrides of BindingBehaviorBase

        /// <summary>
        ///     Gets the id of behavior. Each <see cref="IDataBinding" /> can have only one instance with the same id.
        /// </summary>
        public override Guid Id
        {
            get { return IdDelayBindingBehavior; }
        }

        /// <summary>
        ///     Gets the behavior priority.
        /// </summary>
        public override int Priority
        {
            get { return 0; }
        }

        /// <summary>
        ///     Attaches to the specified binding.
        /// </summary>
        protected override bool OnAttached()
        {
            Binding.SourceAccessor.ValueChanging += SourceOnValueChanging;
            _timer = new Timer(CallbackInternalDelegate, this, int.MaxValue, int.MaxValue);
            return true;
        }

        /// <summary>
        ///     Detaches this instance from its associated binding.
        /// </summary>
        protected override void OnDetached()
        {
            _timer.Dispose();
            Binding.SourceAccessor.ValueChanging -= SourceOnValueChanging;
        }

        /// <summary>
        ///     Creates a new binding behavior that is a copy of the current instance.
        /// </summary>
        protected override IBindingBehavior CloneInternal()
        {
            return new DelayBindingBehavior((uint)Delay);
        }

        #endregion

        #region Methods

        private void SourceOnValueChanging(IBindingSourceAccessor sender, ValueAccessorChangingEventArgs args)
        {
            if (args.Cancel || _isUpdating)
                return;
            args.Cancel = true;
            _timer.Change(Delay, int.MaxValue);
            _context = SynchronizationContext.Current;
        }

        private static void CallbackInternal(object state)
        {
            var behavior = (DelayBindingBehavior)state;
            if (behavior._context == null)
                ToolkitExtensions.InvokeOnUiThreadAsync(behavior.Callback);
            else
                behavior._context.Post(CallbackDelegate, state);
        }

        private static void Callback(object state)
        {
            ((DelayBindingBehavior)state).Callback();
        }

        private void Callback()
        {
            try
            {
                _isUpdating = true;
                _timer.Change(int.MaxValue, int.MaxValue);
                var binding = Binding;
                if (binding != null)
                    binding.UpdateSource();
            }
            finally
            {
                _isUpdating = false;
            }
        }

        #endregion
    }
}