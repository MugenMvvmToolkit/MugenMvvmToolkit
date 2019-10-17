using System;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Parsing.Expressions
{
    public class BindingMemberExpression : ParameterExpression, IBindingMemberExpression
    {
        #region Fields

        private readonly string? _observableMethodName;
        private readonly IObserverProvider? _observerProvider;
        private IMemberPath? _dataContextMemberPath;
        private IMemberPath? _memberPath;

        #endregion

        #region Constructors

        public BindingMemberExpression(string path, MemberFlags memberFlags, string? observableMethodName, IObserverProvider? observerProvider = null)
            : base(path, -1)
        {
            _observableMethodName = observableMethodName;
            _observerProvider = observerProvider;
            MemberFlags = memberFlags;
        }

        #endregion

        #region Properties

        public bool HasStablePath { get; set; }

        public bool Observable { get; set; }

        public bool Optional { get; set; }

        public MemberFlags MemberFlags { get; set; }

        public override ExpressionNodeType NodeType => ExpressionNodeType.BindingMember;

        #endregion

        #region Implementation of interfaces

        public void SetIndex(int index)
        {
            Index = index;
        }

        public virtual IMemberPathObserver GetObserver(object target, object? source, IReadOnlyMetadataContext? metadata)
        {
            var provider = _observerProvider.ServiceIfNull();
            var path = GetPath(provider, source, metadata);
            return provider.GetMemberPathObserver(source ?? target,
                new MemberPathObserverRequest(source ?? target, path, MemberFlags, _observableMethodName, HasStablePath, Observable, Optional), metadata);
        }

        #endregion

        #region Methods

        protected IMemberPath GetPath(IObserverProvider provider, object? source, IReadOnlyMetadataContext? metadata)
        {
            if (source == null)
            {
                if (_dataContextMemberPath != null)
                    return _dataContextMemberPath;

                string path;
                if (Name.StartsWith("[", StringComparison.Ordinal))
                    path = GetEmptySourcePath() + Name;
                else
                    path = GetEmptySourcePath() + "." + Name;
                _dataContextMemberPath = provider.GetMemberPath(path, metadata);
                return _dataContextMemberPath;
            }

            if (_memberPath == null)
                _memberPath = provider.GetMemberPath(Name, metadata);
            return _memberPath;
        }

        protected virtual string GetEmptySourcePath()
        {
            return BindableMembers.Object.DataContext;
        }

        #endregion
    }
}