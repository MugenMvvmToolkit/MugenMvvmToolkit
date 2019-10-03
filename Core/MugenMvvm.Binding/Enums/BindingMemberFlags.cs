using MugenMvvm.Enums;

namespace MugenMvvm.Binding.Enums
{
    public class BindingMemberFlags : MemberFlags
    {
        #region Fields

        public static readonly MemberFlags Attached = new MemberFlags(1 << 4);
        public static readonly MemberFlags Dynamic = new MemberFlags(1 << 4);
        public new static readonly MemberFlags All = Static | Instance | Public | NonPublic | Attached | Dynamic;

        #endregion

        #region Constructors

        protected BindingMemberFlags()
        {
        }

        #endregion
    }
}