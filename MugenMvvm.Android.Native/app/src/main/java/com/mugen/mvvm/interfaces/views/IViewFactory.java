package com.mugen.mvvm.interfaces.views;

import androidx.annotation.NonNull;

import java.lang.reflect.InvocationTargetException;

public interface IViewFactory {
    @NonNull
    Object getView(@NonNull Object container, int resourceId, boolean trackLifecycle) throws NoSuchMethodException, IllegalAccessException, InvocationTargetException, InstantiationException;
}
