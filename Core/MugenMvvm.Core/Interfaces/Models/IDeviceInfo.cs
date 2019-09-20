﻿using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Models
{
    public interface IDeviceInfo : IMetadataOwner<IMetadataContext>
    {
        PlatformType Platform { get; }

        PlatformIdiom Idiom { get; }

        string RawVersion { get; }
    }
}