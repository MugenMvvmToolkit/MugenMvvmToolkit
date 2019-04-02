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
    [Serializable]
    [DataContract(Namespace = BuildConstants.DataContractNamespace)]
    [Preserve(Conditional = true, AllMembers = true)]
    public sealed class NavigationCallback<T> : INavigationCallback<T>, INavigationCallbackInternal
    {
        #region Fields

        [DataMember(Name = "C")]
        internal NavigationCallbackType CallbackType;

        [DataMember(Name = "S")]
        internal bool IsSerializable;

        [DataMember(Name = "I")]
        internal string NavigationProviderId;

        [DataMember(Name = "N")]
        internal NavigationType NavigationType;

        [DataMember(Name = "T")]
        internal TaskCompletionSource<T> TaskCompletionSource;

        #endregion

        #region Constructors

        public NavigationCallback(NavigationCallbackType callbackType, NavigationType navigationType, bool isSerializable, string navigationProviderId)
        {
            CallbackType = callbackType;
            NavigationType = navigationType;
            IsSerializable = isSerializable;
            NavigationProviderId = navigationProviderId;
            TaskCompletionSource = new TaskCompletionSource<T>();
        }

        internal NavigationCallback()
        {
        }

        #endregion

        #region Properties

        string INavigationCallback.NavigationProviderId => NavigationProviderId;

        [IgnoreDataMember]
        [XmlIgnore]
        NavigationCallbackType INavigationCallback.CallbackType => CallbackType;

        [IgnoreDataMember]
        [XmlIgnore]
        NavigationType INavigationCallback.NavigationType => NavigationType;

        [IgnoreDataMember]
        [XmlIgnore]
        bool INavigationCallbackInternal.IsSerializable => IsSerializable && !TaskCompletionSource.Task.IsCompleted;

        #endregion

        #region Implementation of interfaces

        Task<T> INavigationCallback<T>.WaitAsync()
        {
            return TaskCompletionSource.Task;
        }

        Task INavigationCallback.WaitAsync()
        {
            return TaskCompletionSource.Task;
        }

        void INavigationCallbackInternal.SetResult(object? result, INavigationContext? navigationContext)
        {
            if (result == null)
                TaskCompletionSource.SetResult(default);
            else
                TaskCompletionSource.TrySetResult((T) result);
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