package com.mugen.mvvm.interfaces.views;

import com.mugen.mvvm.interfaces.views.IViewAttributeAccessor;

public interface IViewBindCallback {
    void bind(Object view, IViewAttributeAccessor bindAttrs);
}
