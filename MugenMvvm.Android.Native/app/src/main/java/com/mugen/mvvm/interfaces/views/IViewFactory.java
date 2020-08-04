package com.mugen.mvvm.interfaces.views;

import java.lang.reflect.InvocationTargetException;

public interface IViewFactory {
    Object getView(Object container, int resourceId, boolean trackLifecycle) throws NoSuchMethodException, IllegalAccessException, InvocationTargetException, InstantiationException;
}
