using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Tests.Components;

namespace MugenMvvm.Tests.Extensions
{
    public static class MugenUnitTestExtensions
    {
        public static T SealedConfiguration<T>(this T owner) where T : class, IComponentOwner
        {
            owner.Components.AddComponent(SealedComponentConfigurationChecker.Instance);
            return owner;
        }
    }
}