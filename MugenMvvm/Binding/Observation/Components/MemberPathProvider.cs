using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Binding.Interfaces.Observation.Components;
using MugenMvvm.Binding.Observation.Paths;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Observation.Components
{
    public sealed class MemberPathProvider : IMemberPathProviderComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = ObserverComponentPriority.PathProvider;

        #endregion

        #region Implementation of interfaces

        public IMemberPath? TryGetMemberPath(IObservationManager observationManager, object path, IReadOnlyMetadataContext? metadata)
        {
            if (!(path is string stringPath))
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