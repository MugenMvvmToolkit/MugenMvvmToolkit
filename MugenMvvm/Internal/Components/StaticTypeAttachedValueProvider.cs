using System;
using MugenMvvm.Collections;
using MugenMvvm.Collections.Internal;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Internal.Components
{
    public sealed class StaticTypeAttachedValueProvider : AttachedValueProviderBase, IHasPriority
    {
        #region Fields

        private readonly MemberInfoLightDictionary<Type, LightDictionary<string, object?>> _attachedValues;

        #endregion

        #region Constructors

        public StaticTypeAttachedValueProvider()
        {
            _attachedValues = new MemberInfoLightDictionary<Type, LightDictionary<string, object?>>(7);
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = InternalComponentPriority.StaticTypeAttachedValueProvider;

        #endregion

        #region Methods

        public override bool IsSupported(object item, IReadOnlyMetadataContext? metadata) => item is Type;

        protected override LightDictionary<string, object?>? GetAttachedDictionary(object item, bool optional)
        {
            var type = (Type) item;
            lock (_attachedValues)
            {
                if (_attachedValues.TryGetValue(type, out var result) || optional)
                    return result;

                result = new LightDictionary<string, object?>();
                _attachedValues[type] = result;
                return result;
            }
        }

        protected override bool ClearInternal(object item)
        {
            lock (_attachedValues)
            {
                return _attachedValues.Remove((Type) item);
            }
        }

        #endregion
    }
}