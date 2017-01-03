using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.iOS.Interfaces
{
    public interface INativeObjectManager
    {
        void Initialize([CanBeNull] object item, [CanBeNull] IDataContext context);

        void Dispose([CanBeNull] object item, [CanBeNull] IDataContext context);
    }
}