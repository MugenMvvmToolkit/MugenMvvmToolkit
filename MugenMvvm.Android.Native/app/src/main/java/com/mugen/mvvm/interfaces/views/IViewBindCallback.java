package com.mugen.mvvm.interfaces.views;

public interface IViewBindCallback {
    void onSetView(Object owner, Object view);

    void bind(Object view, IViewAttributeAccessor bindAttrs);
}
