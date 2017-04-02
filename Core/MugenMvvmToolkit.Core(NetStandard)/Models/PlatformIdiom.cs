namespace MugenMvvmToolkit.Models
{
    public class PlatformIdiom : StringConstantBase<PlatformIdiom>
    {
        #region Fields

        public static readonly PlatformIdiom Desktop;

        public static readonly PlatformIdiom Tablet;

        public static readonly PlatformIdiom Phone;

        public static readonly PlatformIdiom Unknown;

        public static readonly PlatformIdiom Car;

        public static readonly PlatformIdiom TV;

        #endregion

        #region Constructors

        static PlatformIdiom()
        {
            Desktop = new PlatformIdiom(nameof(Desktop));
            Tablet = new PlatformIdiom(nameof(Tablet));
            Phone = new PlatformIdiom(nameof(Phone));
            Car = new PlatformIdiom(nameof(Car));
            TV = new PlatformIdiom(nameof(TV));
            Unknown = new PlatformIdiom(nameof(Unknown));
        }

        public PlatformIdiom(string id) : base(id)
        {
        }

        #endregion
    }
}