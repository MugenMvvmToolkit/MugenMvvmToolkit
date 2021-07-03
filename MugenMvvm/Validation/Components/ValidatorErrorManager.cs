using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;

namespace MugenMvvm.Validation.Components
{
    public sealed class ValidatorErrorManager : IValidatorErrorManagerComponent
    {
        private readonly Dictionary<CacheKey, List<ValidationErrorInfo>> _errors;

        public ValidatorErrorManager()
        {
            _errors = new Dictionary<CacheKey, List<ValidationErrorInfo>>();
        }

        private static bool RemoveError(List<ValidationErrorInfo> errors, ValidationErrorInfo error)
        {
            for (var i = 0; i < errors.Count; i++)
            {
                var e = errors[i];
                if (Equals(e.Target, error.Target) && Equals(e.Error, error.Error))
                {
                    errors.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Add(ref ItemOrListEditor<string> members, string member)
        {
            if (!members.Contains(member))
                members.Add(member);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool HasErrors(KeyValuePair<CacheKey, List<ValidationErrorInfo>> error, object? source, string member) =>
            error.Value.Count != 0 && (source == null || source.Equals(error.Key.Source)) && (member == "" || error.Key.Member == member);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void AddErrors(ref ItemOrListEditor<object> errors, List<ValidationErrorInfo> value)
        {
            for (var i = 0; i < value.Count; i++)
                errors.Add(value[i].Error!);
        }

        public bool HasErrors(IValidator validator, ItemOrIReadOnlyList<string> members, object? source, IReadOnlyMetadataContext? metadata)
        {
            lock (_errors)
            {
                if (_errors.Count == 0)
                    return false;

                if (members.IsEmpty || source == null)
                {
                    if (members.IsEmpty)
                        members = "";
                    foreach (var error in _errors)
                    foreach (var member in members)
                    {
                        if (HasErrors(error, source, member))
                            return true;
                    }

                    return false;
                }

                foreach (var member in members)
                {
                    if (_errors.TryGetValue(new CacheKey(source, member), out var v) && v.Count != 0)
                        return true;
                }

                return false;
            }
        }

        public void GetErrors(IValidator validator, ItemOrIReadOnlyList<string> members, ref ItemOrListEditor<ValidationErrorInfo> errors, object? source,
            IReadOnlyMetadataContext? metadata)
        {
            lock (_errors)
            {
                if (_errors.Count == 0)
                    return;

                if (members.IsEmpty || source == null)
                {
                    if (members.IsEmpty)
                        members = "";
                    foreach (var error in _errors)
                    foreach (var member in members)
                    {
                        if (HasErrors(error, source, member))
                            errors.AddRange(value: error.Value);
                    }
                }
                else
                {
                    foreach (var member in members)
                    {
                        if (_errors.TryGetValue(new CacheKey(source, member), out var v))
                            errors.AddRange(value: v);
                    }
                }
            }
        }

        public void GetErrors(IValidator validator, ItemOrIReadOnlyList<string> members, ref ItemOrListEditor<object> errors, object? source, IReadOnlyMetadataContext? metadata)
        {
            lock (_errors)
            {
                if (_errors.Count == 0)
                    return;

                if (members.IsEmpty || source == null)
                {
                    if (members.IsEmpty)
                        members = "";
                    foreach (var error in _errors)
                    foreach (var member in members)
                    {
                        if (HasErrors(error, source, member))
                            AddErrors(ref errors, error.Value);
                    }
                }
                else
                {
                    foreach (var member in members)
                    {
                        if (_errors.TryGetValue(new CacheKey(source, member), out var v))
                            AddErrors(ref errors, v);
                    }
                }
            }
        }

        public void SetErrors(IValidator validator, object source, ItemOrIReadOnlyList<ValidationErrorInfo> errors, IReadOnlyMetadataContext? metadata)
        {
            var toNotify = new ItemOrListEditor<string>(2);
            lock (_errors)
            {
                foreach (var error in errors)
                {
                    if (error.HasError && (!_errors.TryGetValue(new CacheKey(source, error.Member), out var value) || value.Count == 0 || !RemoveError(value, error)))
                        Add(ref toNotify, error.Member);
                }

                foreach (var error in errors)
                {
                    if (_errors.TryGetValue(new CacheKey(source, error.Member), out var value) && value.Count != 0)
                    {
                        Add(ref toNotify, error.Member);
                        value.Clear();
                    }
                }

                foreach (var error in errors)
                {
                    if (!error.HasError)
                        continue;

                    var key = new CacheKey(source, error.Member);
                    if (!_errors.TryGetValue(key, out var value))
                    {
                        value = new List<ValidationErrorInfo>(2);
                        _errors[key] = value;
                    }

                    value.Add(error);
                }
            }

            if (toNotify.Count != 0)
                validator.GetComponents<IValidatorErrorsChangedListener>().OnErrorsChanged(validator, toNotify, metadata);
        }

        public void ClearErrors(IValidator validator, ItemOrIReadOnlyList<string> members, object? source, IReadOnlyMetadataContext? metadata)
        {
            var toNotify = new ItemOrListEditor<string>(2);
            lock (_errors)
            {
                if (members.IsEmpty || source == null)
                {
                    if (members.IsEmpty)
                        members = "";
                    foreach (var error in _errors)
                    foreach (var member in members)
                    {
                        if (HasErrors(error, source, member))
                        {
                            error.Value.Clear();
                            Add(ref toNotify, error.Key.Member);
                        }
                    }
                }
                else
                {
                    foreach (var member in members)
                    {
                        if (_errors.TryGetValue(new CacheKey(source, member), out var v) && v.Count != 0)
                        {
                            v.Clear();
                            Add(ref toNotify, member);
                        }
                    }
                }
            }

            if (toNotify.Count != 0)
                validator.GetComponents<IValidatorErrorsChangedListener>().OnErrorsChanged(validator, toNotify, metadata);
        }

        [StructLayout(LayoutKind.Auto)]
        internal readonly struct CacheKey : IEquatable<CacheKey>
        {
            public readonly object Source;
            public readonly string Member;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public CacheKey(object source, string member)
            {
                Should.NotBeNull(source, nameof(source));
                Should.NotBeNull(member, nameof(member));
                Source = source;
                Member = member;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Equals(CacheKey other) => Source.Equals(other.Source) && Member == other.Member;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override int GetHashCode() => HashCode.Combine(Source, Member);
        }
    }
}