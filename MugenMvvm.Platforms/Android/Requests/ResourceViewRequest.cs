﻿using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Requests;

namespace MugenMvvm.Android.Requests
{
    public class ResourceViewRequest : ViewModelViewRequest
    {
        public ResourceViewRequest(IViewModelBase viewModel, object container, int resourceId)
            : base(viewModel, null)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(container, nameof(container));
            Container = container;
            ResourceId = resourceId;
        }

        public object Container { get; protected set; }

        public int ResourceId { get; protected set; }
    }
}