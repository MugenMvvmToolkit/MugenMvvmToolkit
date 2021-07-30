using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Validation;

namespace MugenMvvm.Validation
{
    [StructLayout(LayoutKind.Auto)]
    public ref struct ValidationRuleMemberBuilder<T, TMember> where T : class
    {
        public readonly Func<T, TMember> Accessor;
        public readonly string MemberName;
        public ValidationRuleBuilder.Builder<T> Builder;

        public ValidationRuleMemberBuilder(string memberName, Func<T, TMember> accessor, ValidationRuleBuilder.Builder<T> builder)
        {
            Should.NotBeNull(memberName, nameof(memberName));
            Should.NotBeNull(accessor, nameof(accessor));
            Accessor = accessor;
            MemberName = memberName;
            Builder = builder;
        }

        public ItemOrIReadOnlyList<IValidationRule> Build() => Builder.Build();

        public ValidationRuleMemberBuilder<T, TMember> Must(Func<T, TMember, IReadOnlyMetadataContext?, bool> validator, ValidationError error,
            Func<T, IReadOnlyMetadataContext?, bool>? condition = null, ItemOrIReadOnlyList<string> dependencyMembers = default)
        {
            Should.NotBeNull(validator, nameof(validator));
            Builder.AddValidator(MemberName, Accessor, (validator, error, condition), (t, v, s, m) => s.validator(t, v, m) ? null : s.error.Error,
                condition == null ? null : (t, s, m) => s.condition!(t, m), dependencyMembers);
            return this;
        }

        public ValidationRuleMemberBuilder<T, TMember> MustAsync(Func<T, TMember, CancellationToken, IReadOnlyMetadataContext?, Task<bool>> validator, ValidationError error,
            Func<T, IReadOnlyMetadataContext?, bool>? condition = null, ItemOrIReadOnlyList<string> dependencyMembers = default)
        {
            Should.NotBeNull(validator, nameof(validator));
            Builder.AddAsyncValidator(MemberName, Accessor, (validator, error, condition),
                (t, v, s, c, m) => s.validator(t, v, c, m).ContinueWith((task, o) => task.Result ? null : new ValidationError(o).Error, s.error.ErrorRaw, c,
                    TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current), condition == null ? null : (t, s, m) => s.condition!(t, m), dependencyMembers);
            return this;
        }
    }
}