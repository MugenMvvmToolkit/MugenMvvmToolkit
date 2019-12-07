﻿using System.Runtime.CompilerServices;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Parsing.Expressions.Binding
{
    public abstract class BindingMemberExpressionNodeBase : ExpressionNodeBase, IBindingMemberExpressionNode
    {
        #region Fields

        protected readonly IObserverProvider? ObserverProvider;
        private IMemberPath? _memberPath;

        #endregion

        #region Constructors

        protected BindingMemberExpressionNodeBase(string path, IObserverProvider? observerProvider)
        {
            Should.NotBeNull(path, nameof(path));
            Path = path;
            ObserverProvider = observerProvider;
        }

        #endregion

        #region Properties

        public override ExpressionNodeType ExpressionType => ExpressionNodeType.BindingMember;

        public BindingMemberExpressionFlags Flags { get; set; }

        public int Index { get; set; }

        public MemberFlags MemberFlags { get; set; }

        public string? ObservableMethodName { get; set; }

        public string Path { get; }

        #endregion

        #region Implementation of interfaces

        public abstract object GetTarget(object target, object? source, IReadOnlyMetadataContext? metadata, out IMemberPath path, out MemberFlags memberFlags);

        public abstract object GetSource(object target, object? source, IReadOnlyMetadataContext? metadata, out IMemberPath path, out MemberFlags memberFlags);

        public abstract IMemberPathObserver GetBindingTarget(object target, object? source, IReadOnlyMetadataContext? metadata);

        public abstract object? GetBindingSource(object target, object? source, IReadOnlyMetadataContext? metadata);

        #endregion

        #region Methods

        protected override IExpressionNode VisitInternal(IExpressionVisitor visitor, IReadOnlyMetadataContext? metadata)
        {
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected IMemberPath GetMemberPath(IReadOnlyMetadataContext? metadata)
        {
            if (_memberPath == null)
                _memberPath = ObserverProvider.DefaultIfNull().GetMemberPath(Path, metadata);
            return _memberPath;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected IMemberPathObserver GetObserver(object target, IMemberPath memberPath, IReadOnlyMetadataContext? metadata)
        {
            var request = new MemberPathObserverRequest(memberPath, MemberFlags,
                Flags.HasFlagEx(BindingMemberExpressionFlags.ObservableMethod) ? ObservableMethodName : null, Flags.HasFlagEx(BindingMemberExpressionFlags.StablePath),
                Flags.HasFlagEx(BindingMemberExpressionFlags.Observable), Flags.HasFlagEx(BindingMemberExpressionFlags.StablePath));
            return ObserverProvider.DefaultIfNull().GetMemberPathObserver(target, request, metadata);
        }

        public override string ToString()
        {
            return $"bind{Index}({Path})";
        }

        #endregion
    }
}