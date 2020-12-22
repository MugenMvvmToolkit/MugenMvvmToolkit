using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.App.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTests.App.Internal
{
    public class TestUnhandledExceptionHandlerComponent : IUnhandledExceptionHandlerComponent, IHasPriority
    {
        #region Fields

        private readonly IMugenApplication? _mugenApplication;

        #endregion

        #region Constructors

        public TestUnhandledExceptionHandlerComponent(IMugenApplication? mugenApplication = null)
        {
            _mugenApplication = mugenApplication;
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        public Action<Exception, UnhandledExceptionType, IReadOnlyMetadataContext?>? OnUnhandledException { get; set; }

        #endregion

        #region Implementation of interfaces

        void IUnhandledExceptionHandlerComponent.OnUnhandledException(IMugenApplication application, Exception exception, UnhandledExceptionType type, IReadOnlyMetadataContext? metadata)
        {
            _mugenApplication?.ShouldEqual(application);
            OnUnhandledException?.Invoke(exception, type, metadata);
        }

        #endregion
    }
}