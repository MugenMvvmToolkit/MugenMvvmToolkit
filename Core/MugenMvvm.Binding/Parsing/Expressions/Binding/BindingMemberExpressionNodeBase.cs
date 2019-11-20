using System.Runtime.CompilerServices;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Parsing.Expressions.Binding
{
    public abstract class BindingMemberExpressionNodeBase : ParameterExpressionNode, IBindingMemberExpressionNode
    {
        #region Fields

        protected readonly IObserverProvider? ObserverProvider;
        private IMemberPath? _memberPath;

        #endregion

        #region Constructors

        protected BindingMemberExpressionNodeBase(string path, IObserverProvider? observerProvider) : base(path, -1)
        {
            ObserverProvider = observerProvider;
        }

        #endregion

        #region Properties

        public override ExpressionNodeType NodeType => ExpressionNodeType.BindingMember;

        public BindingMemberExpressionFlags Flags { get; set; }

        public MemberFlags MemberFlags { get; set; }

        public string? ObservableMethodName { get; set; }

        #endregion

        #region Implementation of interfaces

        public virtual void SetIndex(int index)
        {
            Index = index;
        }

        public abstract object GetTarget(object target, object? source, IReadOnlyMetadataContext? metadata, out IMemberPath path, out MemberFlags memberFlags);

        public abstract object GetSource(object target, object? source, IReadOnlyMetadataContext? metadata, out IMemberPath path, out MemberFlags memberFlags);

        public abstract IMemberPathObserver GetTargetObserver(object target, object? source, IReadOnlyMetadataContext? metadata);

        public abstract IMemberPathObserver GetSourceObserver(object target, object? source, IReadOnlyMetadataContext? metadata);

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected IMemberPath GetMemberPath(IReadOnlyMetadataContext? metadata)
        {
            if (_memberPath == null)
                _memberPath = ObserverProvider.DefaultIfNull().GetMemberPath(Name, metadata);
            return _memberPath;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected IMemberPathObserver GetObserver(object target, IMemberPath memberPath, IReadOnlyMetadataContext? metadata)
        {
            var request = new MemberPathObserverRequest(memberPath, MemberFlags,
                CheckFlags(BindingMemberExpressionFlags.ObservableMethod) ? ObservableMethodName : null, CheckFlags(BindingMemberExpressionFlags.StablePath),
                CheckFlags(BindingMemberExpressionFlags.Observable), CheckFlags(BindingMemberExpressionFlags.StablePath));
            return ObserverProvider.DefaultIfNull().GetMemberPathObserver(target, request, metadata);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool CheckFlags(BindingMemberExpressionFlags flag)
        {
            return Flags.HasFlagEx(flag);
        }

        #endregion
    }
}