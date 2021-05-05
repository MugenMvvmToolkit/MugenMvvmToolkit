using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Collections.Components
{
    public class HeaderFooterCollectionDecorator : AttachableComponentBase<ICollection>, ICollectionDecorator, IHasPriority
    {
        private ICollectionDecoratorManagerComponent? _decoratorManager;
        private object? _footer;
        private object? _header;
        private int _footerIndex;

        public HeaderFooterCollectionDecorator(int priority = CollectionComponentPriority.HeaderFooterDecorator)
        {
            Priority = priority;
            _footerIndex = -1;
        }

        public object? Header
        {
            get => _header;
            set
            {
                if (Equals(_header, value))
                    return;
                using var l = OwnerOptional.TryLock();
                var oldValue = _header;
                _header = value;
                if (_decoratorManager != null)
                    UpdateHeader(value, oldValue);
            }
        }

        public object? Footer
        {
            get => _footer;
            set
            {
                if (Equals(_footer, value))
                    return;
                using var l = OwnerOptional.TryLock();
                var oldValue = _footer;
                _footer = value;
                if (_decoratorManager != null)
                    UpdateFooter(value, oldValue);
            }
        }

        public int Priority { get; }

        protected override void OnAttached(ICollection owner, IReadOnlyMetadataContext? metadata)
        {
            _decoratorManager = CollectionDecoratorManager.GetOrAdd(owner);
            using var l = owner.TryLock();
            if (_header != null)
                UpdateHeader(Header, null);
            if (_footer != null)
                UpdateFooter(Footer, null);
        }

        protected override void OnDetached(ICollection owner, IReadOnlyMetadataContext? metadata)
        {
            using var l = owner.TryLock();
            if (_header != null)
                UpdateHeader(null, _header);
            if (_footer != null)
                UpdateFooter(null, _footer);
            _decoratorManager = null;
        }

        private IEnumerable<object?> DecorateItems(IEnumerable<object?>? items)
        {
            if (Header != null)
                yield return Header;
            if (items != null)
            {
                foreach (var item in items)
                    yield return item;
            }

            if (Footer != null)
                yield return Footer;
        }

        private void UpdateHeader(object? value, object? oldValue)
        {
            if (value == null)
            {
                _decoratorManager!.OnRemoved(Owner, this, oldValue, 0);
                if (_footerIndex > 0)
                    --_footerIndex;
            }
            else
            {
                if (oldValue == null)
                    _decoratorManager!.OnAdded(Owner, this, value, 0);
                else
                    _decoratorManager!.OnReplaced(Owner, this, oldValue, value, 0);
            }
        }

        private void UpdateFooter(object? value, object? oldValue)
        {
            if (value == null)
            {
                _decoratorManager!.OnRemoved(Owner, this, oldValue, _footerIndex);
                _footerIndex = -1;
            }
            else if (_footerIndex < 0)
            {
                _footerIndex = _decoratorManager!.DecorateItems(Owner, this).Count();
                if (_header != null)
                    ++_footerIndex;
                _decoratorManager.OnAdded(Owner, this, value, _footerIndex);
            }
            else
                _decoratorManager!.OnReplaced(Owner, this, oldValue, value, _footerIndex);
        }

        IEnumerable<object?> ICollectionDecorator.DecorateItems(ICollection collection, IEnumerable<object?> items) => DecorateItems(items);

        bool ICollectionDecorator.OnItemChanged(ICollection collection, ref object? item, ref int index, ref object? args)
        {
            if (_header != null)
                ++index;
            return true;
        }

        bool ICollectionDecorator.OnAdded(ICollection collection, ref object? item, ref int index)
        {
            if (_header != null)
                ++index;
            if (_footerIndex > 0 && index >= _footerIndex)
            {
                if (index > _footerIndex)
                    --index;
                _footerIndex++;
            }

            return true;
        }

        bool ICollectionDecorator.OnReplaced(ICollection collection, ref object? oldItem, ref object? newItem, ref int index)
        {
            if (_header != null)
                ++index;
            return true;
        }

        bool ICollectionDecorator.OnMoved(ICollection collection, ref object? item, ref int oldIndex, ref int newIndex)
        {
            if (_header != null)
            {
                ++oldIndex;
                ++newIndex;
            }

            return true;
        }

        bool ICollectionDecorator.OnRemoved(ICollection collection, ref object? item, ref int index)
        {
            if (_header != null)
                ++index;
            if (_footerIndex > 0)
                --_footerIndex;
            return true;
        }

        bool ICollectionDecorator.OnReset(ICollection collection, ref IEnumerable<object?>? items)
        {
            items = DecorateItems(items);
            return true;
        }
    }
}