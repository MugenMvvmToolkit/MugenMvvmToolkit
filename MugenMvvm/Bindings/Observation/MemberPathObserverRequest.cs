using System.Runtime.CompilerServices;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;

namespace MugenMvvm.Bindings.Observation
{
    public class MemberPathObserverRequest
    {
        #region Fields

        private ushort _memberFlags;

        #endregion

        #region Constructors

        public MemberPathObserverRequest(IMemberPath path, EnumFlags<MemberFlags> memberFlags, string? observableMethodName,
            bool hasStablePath, bool observable, bool optional, IExpressionNode? expression)
        {
            Should.NotBeNull(path, nameof(path));
            Path = path;
            MemberFlags = memberFlags;
            HasStablePath = hasStablePath;
            Observable = observable;
            Optional = optional;
            ObservableMethodName = observable ? observableMethodName : null;
            Expression = expression;
        }

        #endregion

        #region Properties

        public bool HasStablePath { get; protected set; }

        public EnumFlags<MemberFlags> MemberFlags
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new EnumFlags<MemberFlags>(_memberFlags);
            protected set => _memberFlags = value.Value();
        }

        public bool Observable { get; protected set; }

        public string? ObservableMethodName { get; protected set; }

        public bool Optional { get; protected set; }

        public IMemberPath Path { get; protected set; }

        public IExpressionNode? Expression { get; }

        #endregion
    }
}