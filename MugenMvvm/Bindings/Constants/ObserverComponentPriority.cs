﻿using MugenMvvm.Constants;

namespace MugenMvvm.Bindings.Constants
{
    public static class ObservationComponentPriority
    {
        public const int EventObserverProvider = 10;
        public const int PropertyChangedObserverProvider = 0;
        public const int MemberPathObserverProvider = 0;
        public const int PathProvider = 0;
        public const int NonObservableMemberObserverDecorator = 100;
        public const int Cache = ComponentPriority.Cache;
    }
}