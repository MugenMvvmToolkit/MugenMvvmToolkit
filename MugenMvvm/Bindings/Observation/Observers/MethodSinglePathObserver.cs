using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Observation.Observers
{
    public sealed class MethodSinglePathObserver : SinglePathObserver, ObserverBase.IMethodPathObserver
    {
        private readonly string _method;

        private IWeakReference? _lastValueRef;
        private ActionToken _unsubscriber;

        public MethodSinglePathObserver(string method, object target, IMemberPath path, EnumFlags<MemberFlags> memberFlags, bool optional)
            : base(target, path, memberFlags, optional)
        {
            Should.NotBeNull(method, nameof(method));
            _method = method;
        }

        string IMethodPathObserver.Method => _method;

        protected override void SubscribeLastMember(object? target, IMemberInfo? lastMember, IReadOnlyMetadataContext? metadata)
        {
            base.SubscribeLastMember(target, lastMember, metadata);
            this.AddMethodObserver(target, lastMember, metadata, ref _unsubscriber, ref _lastValueRef);
        }

        protected override void OnLastMemberChanged()
        {
            base.OnLastMemberChanged();
            var lastMember = GetLastMember();
            this.AddMethodObserver(lastMember.Target, lastMember.Member, TryGetMetadata(), ref _unsubscriber, ref _lastValueRef);
        }

        protected override void UnsubscribeLastMember()
        {
            _unsubscriber.Dispose();
            base.UnsubscribeLastMember();
        }

        IEventListener IMethodPathObserver.GetMethodListener() => this;
    }
}