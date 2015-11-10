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

        public override IViewModelSettings Clone()
        {
            if (WithoutClone)
                return this;
            var set = new ViewModelSettingsMock
            {
                DisposeCommands = DisposeCommands,
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
