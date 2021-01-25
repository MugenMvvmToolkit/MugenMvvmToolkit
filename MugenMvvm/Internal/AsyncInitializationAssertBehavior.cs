using System;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Members.Components;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Internal
{
    internal sealed class AsyncInitializationAssertBehavior : IComponentCollectionManagerListener, IComponentCollectionDecorator, IMemberManagerComponent,
        IFallbackServiceConfiguration, IHasPriority
    {
        private readonly Func<bool> _isInitializing;
        private readonly Func<object, bool>? _canIgnore;
        private readonly int _threadId;

        public AsyncInitializationAssertBehavior(Func<bool> isInitializing, Func<object, bool>? canIgnore = null)
        {
            Should.NotBeNull(isInitializing, nameof(isInitializing));
            _isInitializing = isInitializing;
            _canIgnore = canIgnore;
            _threadId = Environment.CurrentManagedThreadId;
            MugenService.Configuration.FallbackConfiguration ??= this;
        }

        public int Priority => int.MaxValue;

        public bool CanDecorate<T>(IReadOnlyMetadataContext? metadata) where T : class => _canIgnore == null || !_canIgnore(typeof(T));

        public void Decorate<T>(IComponentCollection collection, ref ItemOrListEditor<T> components, IReadOnlyMetadataContext? metadata) where T : class
        {
            if (!CanDecorate<T>(metadata))
                return;

            if (!Assert())
            {
                collection.RemoveComponent(this);
                return;
            }

            if (typeof(T) == typeof(IMemberManagerComponent))
                components.Add((T) (object) this);
        }

        public void OnComponentCollectionCreated(IComponentCollectionManager collectionManager, IComponentCollection collection, IReadOnlyMetadataContext? metadata)
        {
            if (Assert())
            {
                if (collection.Owner is not IComponentCollection && (_canIgnore == null || !_canIgnore(collection.Owner)))
                    collection.TryAddComponent(this);
            }
            else
                collectionManager.RemoveComponent(this);
        }

        public TService Instance<TService>() where TService : class
        {
            Assert();
            return null!;
        }

        public TService? Optional<TService>() where TService : class
        {
            Assert();
            return null;
        }

        public ItemOrIReadOnlyList<IMemberInfo> TryGetMembers(IMemberManager memberManager, Type type, EnumFlags<MemberType> memberTypes, EnumFlags<MemberFlags> flags,
            object request, IReadOnlyMetadataContext? metadata)
        {
            Assert();
            return default;
        }

        private bool Assert()
        {
            if (!_isInitializing())
            {
                if (MugenService.Configuration.FallbackConfiguration == this)
                    MugenService.Configuration.FallbackConfiguration = null;
                return false;
            }

            if (_threadId != Environment.CurrentManagedThreadId)
                ExceptionManager.ThrowAsyncInitializationAssert();
            return true;
        }
    }
}