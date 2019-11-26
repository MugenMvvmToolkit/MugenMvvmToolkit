using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Navigation;

namespace MugenMvvm.Navigation
{
    [Serializable]
    [DataContract(Namespace = BuildConstant.DataContractNamespace)]
    [Preserve(Conditional = true, AllMembers = true)]
    public sealed class NavigationCallback : INavigationCallback<bool>
    {
        #region Fields

        [DataMember(Name = "I")]
        public readonly string NavigationOperationId;

        [DataMember(Name = "C")]
        public NavigationCallbackType CallbackType;

        [DataMember(Name = "S")]
        public bool IsSerializableField;

        [DataMember(Name = "N")]
        public NavigationType NavigationType;

        [DataMember(Name = "T")]
        public TaskCompletionSource<bool> TaskCompletionSource;

        #endregion

        #region Constructors

        public NavigationCallback(NavigationCallbackType callbackType, NavigationType navigationType, bool isSerializable, string operationId)
        {
            CallbackType = callbackType;
            NavigationType = navigationType;
            IsSerializableField = isSerializable;
            NavigationOperationId = operationId;
            TaskCompletionSource = new TaskCompletionSource<bool>();
        }

#pragma warning disable CS8618
        internal NavigationCallback()
        {
        }
#pragma warning restore CS8618

        #endregion

        #region Properties

        NavigationCallbackType INavigationCallback.CallbackType => CallbackType;

        NavigationType INavigationCallback.NavigationType => NavigationType;

        public bool IsSerializable => IsSerializableField && !TaskCompletionSource.Task.IsCompleted;

        #endregion

        #region Implementation of interfaces

        public Task<bool> WaitAsync()
        {
            return TaskCompletionSource.Task;
        }

        Task INavigationCallback.WaitAsync()
        {
            return WaitAsync();
        }

        #endregion

        #region Methods

        public void SetResult(bool result)
        {
            TaskCompletionSource?.TrySetResult(result);
        }

        public void SetException(Exception exception)
        {
            TaskCompletionSource?.TrySetException(exception);
        }

        public void SetCanceled()
        {
            TaskCompletionSource?.TrySetCanceled();
        }

        #endregion
    }
}