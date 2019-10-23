using System;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Interfaces.Resources;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Parsing.Expressions
{
    public sealed class BindingMemberExpression : ParameterExpression, IBindingMemberExpression
    {
        #region Fields

        private readonly IObserverProvider? _observerProvider;
        private readonly IResourceResolver? _resourceResolver;
        private IMemberPath? _dataContextMemberPath;
        private IMemberPath? _memberPath;

        #endregion

        #region Constructors

        public BindingMemberExpression(string path, IObserverProvider? observerProvider = null, IResourceResolver? resourceResolver = null)
            : base(path, -1)
        {
            _observerProvider = observerProvider;
            _resourceResolver = resourceResolver;
        }

        #endregion

        #region Properties

        public BindingMemberExpressionFlags Flags { get; set; }

        public string? ObservableMethodName { get; set; }

        public MemberFlags MemberFlags { get; set; }

        public override ExpressionNodeType NodeType => ExpressionNodeType.BindingMember;

        public object? Target { get; set; }

        private bool HasStablePath => Flags.HasFlagEx(BindingMemberExpressionFlags.StablePath);

        private bool Observable => Flags.HasFlagEx(BindingMemberExpressionFlags.Observable);

        private bool ObservableMethod => Flags.HasFlagEx(BindingMemberExpressionFlags.ObservableMethod);

        private bool Optional => Flags.HasFlagEx(BindingMemberExpressionFlags.Optional);

        private bool TargetOnly => Flags.HasFlagEx(BindingMemberExpressionFlags.TargetOnly);

        private bool SourceOnly => Flags.HasFlagEx(BindingMemberExpressionFlags.SourceOnly);

        private bool ContextOnly => Flags.HasFlagEx(BindingMemberExpressionFlags.ContextOnly);

        private bool IsResource => Flags.HasFlagEx((BindingMemberExpressionFlags) (1 << 7));

        #endregion

        #region Implementation of interfaces

        public void SetIndex(int index)
        {
            Index = index;
        }

        public IMemberPathObserver GetTargetObserver(object target, object? source, IReadOnlyMetadataContext? metadata)
        {
            return GetSourceObserver(target, target, metadata);
        }

        public IMemberPathObserver GetSourceObserver(object target, object? source, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(target, nameof(target));
            if (IsResource)
            {
                var resourceValue = _resourceResolver.ServiceIfNull().TryGetResourceValue((string) Target);
                if (resourceValue == null)
                    BindingExceptionManager.ThrowCannotResolveResource((string) Target);
                Target = resourceValue;
            }
            else if (TargetOnly)
                source = target;
            else if (SourceOnly)
            {
                if (source == null)
                    source = target;
                else
                    target = source;
            }
            else if (ContextOnly)
                source = null;

            var provider = _observerProvider.ServiceIfNull();
            var request = new MemberPathObserverRequest(GetPath(provider, source, metadata), MemberFlags, ObservableMethod ? ObservableMethodName : null, HasStablePath, Observable,
                Optional);
            return provider.GetMemberPathObserver(Target ?? (source ?? target).ToWeakReference(), request, metadata);
        }

        #endregion

        #region Methods

        private IMemberPath GetPath(IObserverProvider provider, object? source, IReadOnlyMetadataContext? metadata)
        {
            if (source == null)
            {
                if (_dataContextMemberPath != null)
                    return _dataContextMemberPath;

                string path;
                if (Name.StartsWith("[", StringComparison.Ordinal))
                    path = BindableMembers.Object.DataContext + Name;
                else
                    path = BindableMembers.Object.DataContext + "." + Name;
                _dataContextMemberPath = provider.GetMemberPath(path, metadata);
                return _dataContextMemberPath;
            }

            if (_memberPath == null)
                _memberPath = provider.GetMemberPath(Name, metadata);
            return _memberPath;
        }

        #endregion
    }
}