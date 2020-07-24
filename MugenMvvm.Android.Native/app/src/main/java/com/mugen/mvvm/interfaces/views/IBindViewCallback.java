package com.mugen.mvvm.interfaces.views;

public interface IBindViewCallback {
    void onSetView(Object owner, Object view);

    void bind(Object view, IViewAttributeAccessor bindAttrs);
}
