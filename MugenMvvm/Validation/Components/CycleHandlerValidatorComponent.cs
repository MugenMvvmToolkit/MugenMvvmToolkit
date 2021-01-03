using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Validation.Components
{
    public sealed class CycleHandlerValidatorComponent : ComponentDecoratorBase<IValidator, IValidatorComponent>, IValidatorComponent
    {
        #region Fields

        private readonly HashSet<string> _validatingMembers;
        private readonly Dictionary<string, CancellationTokenSource> _validatingTasks;

        #endregion

        #region Constructors

        public CycleHandlerValidatorComponent(int priority = ValidationComponentPriority.CycleHandlerDecorator) : base(priority)
        {
            _validatingTasks = new Dictionary<string, CancellationTokenSource>(3, StringComparer.Ordinal);
            _validatingMembers = new HashSet<string>(StringComparer.Ordinal);
        }

        #endregion

        #region Implementation of interfaces

        public bool HasErrors(IValidator validator, string? memberName, IReadOnlyMetadataContext? metadata)
        {
            if (_validatingTasks.Count != 0)
                return true;
            return Components.HasErrors(validator, memberName, metadata);
        }

        public ItemOrList<object, IReadOnlyList<object>> TryGetErrors(IValidator validator, string? memberName, IReadOnlyMetadataContext? metadata) => Components.TryGetErrors(validator, memberName, metadata);

        public IReadOnlyDictionary<string, object>? TryGetErrors(IValidator validator, IReadOnlyMetadataContext? metadata) => Components.TryGetErrors(validator, metadata);

        public Task TryValidateAsync(IValidator validator, string? memberName, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            var member = memberName ?? "";
            try
            {
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, default);
                lock (_validatingMembers)
                {
                    if (!_validatingMembers.Add(member))
                        return Default.CompletedTask;
                }

                CancellationTokenSource? oldValue;
                lock (_validatingTasks)
                {
                    if (_validatingTasks.TryGetValue(member, out oldValue))
                        _validatingTasks.Remove(member);
                }

                oldValue?.Cancel();

                var task = Components.TryValidateAsync(validator, memberName, source.Token, metadata);
                if (!task.IsCompleted)
                {
                    lock (_validatingTasks)
                    {
                        _validatingTasks.TryGetValue(member, out oldValue);
                        _validatingTasks[member] = source;
                    }

                    oldValue?.Cancel();

                    task.ContinueWith((_, s) =>
                    {
                        var state = (Tuple<CycleHandlerValidatorComponent, string, CancellationTokenSource, IReadOnlyMetadataContext?>) s!;
                        state.Item1.OnAsyncValidationCompleted(state.Item2, state.Item3, state.Item4);
                    }, Tuple.Create(this, member, source, metadata), TaskContinuationOptions.ExecuteSynchronously);
                }

                return task;
            }
            finally
            {
                lock (_validatingMembers)
                {
                    _validatingMembers.Remove(member);
                }
            }
        }

        public void ClearErrors(IValidator validator, string? memberName, IReadOnlyMetadataContext? metadata) => Components.TryGetErrors(validator, memberName, metadata);

        #endregion

        #region Methods

        private void OnAsyncValidationCompleted(string member, CancellationTokenSource cts, IReadOnlyMetadataContext? metadata)
        {
            bool notify;
            lock (_validatingTasks)
            {
                notify = _validatingTasks.TryGetValue(member, out var value) && cts == value && _validatingTasks.Remove(member);
            }

            if (notify)
                OwnerOptional?.GetComponents<IValidatorListener>(metadata).OnErrorsChanged(Owner, null, member, metadata);
        }

        #endregion
    }
}