using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Interfaces.Observation.Components;
using MugenMvvm.Bindings.Observation.Paths;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Observation.Components
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