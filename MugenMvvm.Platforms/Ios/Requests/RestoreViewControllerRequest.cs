using Foundation;
using UIKit;

namespace MugenMvvm.Ios.Requests
{
    public class RestoreViewControllerRequest
    {
        #region Constructors

        public RestoreViewControllerRequest(NSCoder coder, string[] restorationIdentifierComponents)
        {
            Should.NotBeNull(coder, nameof(coder));
            Should.NotBeNull(restorationIdentifierComponents, nameof(restorationIdentifierComponents));
            Coder = coder;
            RestorationIdentifierComponents = restorationIdentifierComponents;
        }

        #endregion

        #region Properties

        public NSCoder Coder { get; }

        public string[] RestorationIdentifierComponents { get; }

        public string RestorationIdentifier => RestorationIdentifierComponents[RestorationIdentifierComponents.Length - 1];

        public UIViewController? ViewController { get; set; }

        #endregion
    }
}