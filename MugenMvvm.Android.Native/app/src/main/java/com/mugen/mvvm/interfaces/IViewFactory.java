package com.mugen.mvvm.interfaces;

public interface IViewFactory {
    Object getView(Object container, int resourceId, boolean trackLifecycle);
}
