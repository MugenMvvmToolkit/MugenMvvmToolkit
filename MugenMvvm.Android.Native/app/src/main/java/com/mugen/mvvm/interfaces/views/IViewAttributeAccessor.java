package com.mugen.mvvm.interfaces.views;

public interface IViewAttributeAccessor {
    String getString(int index);

    int getResourceId(int index);

    String getBind();

    int getItemTemplate();
}
