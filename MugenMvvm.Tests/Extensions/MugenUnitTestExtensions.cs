using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Tests.Components;

namespace MugenMvvm.Tests.Extensions
{
    public static class MugenUnitTestExtensions
    {
        public static void RaisePendingNotifications(this IReadOnlyObservableCollection? collection)
        {
            while (collection != null)
            {
                collection.GetComponents<IHasPendingNotifications>().Raise(null);
                var target = (collection as IHasTarget<IReadOnlyObservableCollection>)?.Target;
                if (!ReferenceEquals(collection, target))
                    collection = target;
            }
        }

        public static T SealedConfiguration<T>(this T owner) where T : class, IComponentOwner
        {
            owner.Components.AddComponent(SealedComponentConfigurationChecker.Instance);
            return owner;
        }
    }
}