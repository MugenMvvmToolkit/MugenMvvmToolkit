package com.mugen.mvvm.interfaces.views;

import androidx.annotation.Nullable;

import java.lang.reflect.InvocationTargetException;

public interface IViewFactory {
    @Nullable
    Object getView(@Nullable Object container, int resourceId, boolean trackLifecycle) throws NoSuchMethodException, IllegalAccessException, InvocationTargetException, InstantiationException;
}
