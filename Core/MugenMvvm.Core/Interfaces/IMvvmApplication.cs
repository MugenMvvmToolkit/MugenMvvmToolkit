using MugenMvvm.Interfaces.Models;
using MugenMvvm.Models;

namespace MugenMvvm.Interfaces
{
    public interface IMvvmApplication
    {        
        ApplicationState State { get; }
        
        IDeviceInfo DeviceInfo { get; }

        void OnStateChanged(ApplicationState value, IReadOnlyContext context);
    }
}