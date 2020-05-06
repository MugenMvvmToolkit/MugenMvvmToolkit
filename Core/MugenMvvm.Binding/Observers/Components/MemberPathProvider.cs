using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Observers.Components;
using MugenMvvm.Binding.Observers.MemberPaths;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Observers.Components
{
    public sealed class MemberPathProvider : IMemberPathProviderComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = ObserverComponentPriority.PathProvider;

        #endregion

        #region Implementation of interfaces

        public IMemberPath? TryGetMemberPath<TPath>(in TPath path, IReadOnlyMetadataContext? metadata)
        {
            if (Default.IsValueType<TPath>() || !(path is string stringPath))
                return null;

            if (stringPath.Length == 0)
                return EmptyMemberPath.Instance;

            var hasBracket = stringPath.IndexOf('[') >= 0;
            if (stringPath.IndexOf('.') >= 0 || hasBracket)
                return new MultiMemberPath(stringPath, hasBracket);
            return new SingleMemberPath(stringPath);
        }

        #endregion
    }
}