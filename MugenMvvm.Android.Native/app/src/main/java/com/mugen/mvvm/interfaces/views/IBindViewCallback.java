package com.mugen.mvvm.interfaces.views;

public interface IBindViewCallback {
    void setViewAccessor(IViewAttributeAccessor accessor);

    void onSetView(Object owner, Object view);

    void bind(Object view);
}
