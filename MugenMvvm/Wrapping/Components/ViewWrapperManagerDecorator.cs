using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Interfaces.Wrapping.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Wrapping.Components
{
    public sealed class ViewWrapperManagerDecorator : ComponentDecoratorBase<IWrapperManager, IWrapperManagerComponent>, IWrapperManagerComponent, IHasPriority//todo test
    {
        #region Properties

        public int Priority { get; set; } = WrappingComponentPriority.ViewDecorator;

        #endregion

        #region Implementation of interfaces

        public bool CanWrap<TRequest>(IWrapperManager wrapperManager, Type wrapperType, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            if (TypeChecker.IsValueType<TRequest>() || !(request is IView view))
                return Components.CanWrap(wrapperManager, wrapperType, request, metadata);
            return wrapperType.IsInstanceOfType(view.Target) || Components.CanWrap(wrapperManager, wrapperType, view.Target, metadata);
        }

        public object? TryWrap<TRequest>(IWrapperManager wrapperManager, Type wrapperType, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            if (TypeChecker.IsValueType<TRequest>() || !(request is IView view))
                return Components.TryWrap(wrapperManager, wrapperType, request, metadata);

            var collection = view.Components;
            lock (collection)
            {
                var item = collection.Get<object>(metadata).FirstOrDefault(wrapperType.IsInstanceOfType);
                if (item == null)
                {
                    item = Components.TryWrap(wrapperManager, wrapperType, view.Target, metadata)!;
                    if (item != null)
                        collection.Add(item);
                }

                return item;
            }
        }

        #endregion
    }
}