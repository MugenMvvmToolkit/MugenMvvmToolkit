using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Navigation;

namespace MugenMvvm.Infrastructure.Navigation
{
    [Serializable, DataContract(Namespace = BuildConstants.DataContractNamespace), Preserve(Conditional = true, AllMembers = true)]
    public sealed class NavigationCallback : INavigationCallback<bool>, INavigationCallbackInternal
    {
        #region Fields

        [DataMember(Name = "C")]
        internal NavigationCallbackType CallbackType;

        [DataMember(Name = "S")]
        internal bool IsSerializable;

        [DataMember(Name = "N")]
        internal NavigationType NavigationType;

        [DataMember(Name = "I")]
        internal string NavigationProviderId;

        [DataMember(Name = "T")]
        internal TaskCompletionSource<bool> TaskCompletionSource;

        #endregion

        #region Constructors

        public NavigationCallback(NavigationCallbackType callbackType, NavigationType navigationType, bool isSerializable, string navigationProviderId)
        {
            CallbackType = callbackType;
            NavigationType = navigationType;
            IsSerializable = isSerializable;
            NavigationProviderId = navigationProviderId;
            TaskCompletionSource = new TaskCompletionSource<bool>();
        }

        internal NavigationCallback()
        {
        }

        #endregion

        #region Properties

        string INavigationCallback.NavigationProviderId => NavigationProviderId;

        [IgnoreDataMember, XmlIgnore]
        NavigationCallbackType INavigationCallback.CallbackType => CallbackType;

        [IgnoreDataMember, XmlIgnore]
        NavigationType INavigationCallback.NavigationType => NavigationType;

        [IgnoreDataMember, XmlIgnore]
        bool INavigationCallbackInternal.IsSerializable => IsSerializable && !TaskCompletionSource.Task.IsCompleted;

        #endregion

        #region Implementation of interfaces

        Task<bool> INavigationCallback<bool>.WaitAsync()
        {
            return TaskCompletionSource.Task;
        }

        Task INavigationCallback.WaitAsync()
        {
            return TaskCompletionSource.Task;
        }

        void INavigationCallbackInternal.SetResult(object result, INavigationContext? navigationContext)
        {
            TaskCompletionSource.TrySetResult((bool?)result ?? false);
        }

        void INavigationCallbackInternal.SetException(Exception exception, INavigationContext? navigationContext)
        {
            TaskCompletionSource.TrySetException(exception);
        }

        void INavigationCallbackInternal.SetCanceled(INavigationContext? navigationContext)
        {
            TaskCompletionSource.TrySetCanceled();
        }

        #endregion
    }
}