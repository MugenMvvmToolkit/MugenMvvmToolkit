using MugenMvvm.Binding.Attributes;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Members;

namespace MugenMvvm.Binding.Interfaces.Build
{
    public interface IBindingBuilderContext
    {
        [BindingMacros(MacrosConstant.Binding)]
        IBinding Binding { get; }

        [BindingMacros(MacrosConstant.EventArgs)]
        T EventArgs<T>();
    }

    public interface IBindingBuilderContext<out TTarget> : IBindingBuilderContext
        where TTarget : class
    {
        [BindingMacros(MacrosConstant.Target)]
        TTarget Target { get; }

        [BindingMacros(MacrosConstant.Target)]
        IBindableMembersBuildingDescriptor<TTarget> TargetEx { get; }
    }

    public interface IBindingBuilderContext<out TTarget, out TSource> : IBindingBuilderContext<TTarget>
        where TTarget : class
        where TSource : class
    {
        [BindingMacros(MacrosConstant.Source)]
        TSource Source { get; }

        [BindingMacros(MacrosConstant.Source)]
        IBindableMembersBuildingDescriptor<TTarget> SourceEx { get; }
    }
}