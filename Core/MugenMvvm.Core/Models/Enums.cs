using System;

namespace MugenMvvm.Models
{
    [Flags]
    public enum MemberFlags
    {
        Static = 1,
        Instance = 2,
        Public = 4,
        NonPublic = 8,
        All = Static | Instance | Public | NonPublic,
        StaticOnly = Static | Public | NonPublic,
        InstanceOnly = Instance | Public | NonPublic,
        InstancePublic = Instance | Public,
        StaticPublic = Static | Public
    }

    [Flags]
    public enum BusyMessageMode
    {
        None = 0,
        Handle = 1,
        NotifySubscribers = 2,
        HandleAndNotifySubscribers = Handle | NotifySubscribers
    }

    public enum SubscriberResult
    {
        Handled = 1,
        Ignored = 2,
        Invalid = 3
    }

    public class ApplicationState : EnumBase<ApplicationState, int>
    {
        #region Fields

        public static readonly ApplicationState Active = new ApplicationState(1);
        public static readonly ApplicationState Background = new ApplicationState(2);

        #endregion

        #region Constructors

        public ApplicationState(int value) : base(value)
        {
        }

        #endregion
    }

    public class ViewModelLifecycleState : EnumBase<ViewModelLifecycleState, int>
    {
        #region Fields

        public static readonly ViewModelLifecycleState Created = new ViewModelLifecycleState(1);
        public static readonly ViewModelLifecycleState Disposed = new ViewModelLifecycleState(2);
        public static readonly ViewModelLifecycleState Finalized = new ViewModelLifecycleState(3);
        public static readonly ViewModelLifecycleState Restored = new ViewModelLifecycleState(4);//todo add more

        #endregion

        #region Constructors

        public ViewModelLifecycleState(int value) : base(value)
        {
        }

        #endregion
    }

    public class PlatformType : EnumBase<PlatformType, string>
    {
        #region Fields

        public static readonly PlatformType Android = new PlatformType(nameof(Android));
        public static readonly PlatformType iOS = new PlatformType(nameof(iOS));
        public static readonly PlatformType WinForms = new PlatformType(nameof(WinForms));
        public static readonly PlatformType UWP = new PlatformType(nameof(UWP));
        public static readonly PlatformType WPF = new PlatformType(nameof(WPF));
        public static readonly PlatformType WinPhone = new PlatformType(nameof(WinPhone));
        public static readonly PlatformType UnitTest = new PlatformType(nameof(UnitTest));
        public static readonly PlatformType Unknown = new PlatformType(nameof(Unknown));

        #endregion

        #region Constructors

        public PlatformType(string id)
            : base(id)
        {
        }

        #endregion

        #region Properties

        public bool IsXamForms { get; private set; }

        #endregion

        #region Methods

        public PlatformType ToXamForms()
        {
            return new PlatformType(Value) { IsXamForms = true };
        }

        #endregion
    }

    public class PlatformIdiom : EnumBase<PlatformIdiom, string>
    {
        #region Fields

        public static readonly PlatformIdiom Desktop = new PlatformIdiom(nameof(Desktop));
        public static readonly PlatformIdiom Tablet = new PlatformIdiom(nameof(Tablet));
        public static readonly PlatformIdiom Phone = new PlatformIdiom(nameof(Phone));
        public static readonly PlatformIdiom Car = new PlatformIdiom(nameof(Car));
        public static readonly PlatformIdiom TV = new PlatformIdiom(nameof(TV));
        public static readonly PlatformIdiom Unknown = new PlatformIdiom(nameof(Unknown));

        #endregion

        #region Constructors

        public PlatformIdiom(string id) : base(id)
        {
        }

        #endregion
    }

    public class ThreadExecutionMode : EnumBase<ThreadExecutionMode, int>
    {
        #region Fields

        public static readonly ThreadExecutionMode Main = new ThreadExecutionMode(1);
        public static readonly ThreadExecutionMode Background = new ThreadExecutionMode(2);
        public static readonly ThreadExecutionMode Current = new ThreadExecutionMode(3);

        #endregion

        #region Constructors

        public ThreadExecutionMode(int value) : base(value)
        {
        }

        #endregion
    }

    public class SerializationMode : EnumBase<ThreadExecutionMode, int>
    {
        #region Fields

        public static readonly SerializationMode Default = new SerializationMode(1);
        public static readonly SerializationMode Clone = new SerializationMode(2);

        #endregion

        #region Constructors

        public SerializationMode(int value) : base(value)
        {
        }

        #endregion
    }
}