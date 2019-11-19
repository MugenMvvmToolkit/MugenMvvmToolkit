using System.Runtime.InteropServices;
using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Core
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct BindingParameterValue
    {
        #region Fields

        public readonly ICompiledExpression? Expression;
        public readonly object? Parameter;

        #endregion

        #region Constructors

        public BindingParameterValue(object? parameter, ICompiledExpression? expression)
        {
            Parameter = parameter;
            Expression = expression;
        }

        #endregion

        #region Properties

        public bool IsEmpty => Parameter == null;

        #endregion

        #region Methods

        public T GetValue<T>(IReadOnlyMetadataContext metadata)
        {
            if (Expression != null)
                return (T)Expression.Invoke(Parameter, metadata);
            if (Parameter is IMemberPathObserver observer)
                return (T)observer.GetLastMember(metadata).GetValue(metadata);
            return (T)Parameter;
        }

        public void Dispose()
        {
            if (Parameter is IMemberPathObserver observer)
                observer.Dispose();
            else if (Parameter is IMemberPathObserver[] observers)
            {
                for (var i = 0; i < observers.Length; i++)
                    observers[i].Dispose();
            }
        }

        #endregion
    }
}