﻿using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Serialization;

namespace MugenMvvm.Models
{
    public class MementoResult : IMementoResult
    {
        #region Fields

        public static readonly IMementoResult Unrestored;

        #endregion

        #region Constructors

        static MementoResult()
        {
            Unrestored = new MementoResult();
        }

        private MementoResult()
        {
            Metadata = Default.MetadataContext;
        }

        public MementoResult(object target, IReadOnlyMetadataContext? metadata = null)
        {
            IsRestored = true;
            Metadata = metadata ?? Default.MetadataContext;
            Target = target;
        }

        #endregion

        #region Properties

        public bool IsRestored { get; }

        public IReadOnlyMetadataContext Metadata { get; }

        public object? Target { get; }

        #endregion
    }
}