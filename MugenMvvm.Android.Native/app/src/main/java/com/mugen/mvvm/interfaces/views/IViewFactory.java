package com.mugen.mvvm.interfaces.views;

public interface IViewFactory {
    Object getView(Object container, int resourceId, boolean trackLifecycle);
}
