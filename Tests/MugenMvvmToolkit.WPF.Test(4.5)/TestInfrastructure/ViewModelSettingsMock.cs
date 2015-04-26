using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    public class ViewModelSettingsMock : DefaultViewModelSettings
    {
        #region Properties

        public bool WithoutClone { get; set; }

        public new IDataContext Metadata
        {
            get { return base.Metadata; }
        }

        #endregion

        #region Overrides of DefaultViewModelSettings

        /// <summary>
        ///     Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        ///     A new object that is a copy of this instance.
        /// </returns>
        public override IViewModelSettings Clone()
        {
            if (WithoutClone)
                return this;
            var set = new ViewModelSettingsMock
            {
                DisposeCommands = DisposeCommands,
                DisposeIocContainer = DisposeIocContainer,
                HandleBusyMessageMode = HandleBusyMessageMode,
                EventExecutionMode = EventExecutionMode,
                DefaultBusyMessage = DefaultBusyMessage,
                BroadcastAllMessages = BroadcastAllMessages,
                WithoutClone = WithoutClone,
                State = new DataContext(State),
            };
            set.Merge(Metadata);
            return set;
        }

        #endregion
    }
}