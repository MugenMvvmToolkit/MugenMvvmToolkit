package com.mugen.mvvm.interfaces.views;

public interface IWrapperFactory {
    int getPriority();

    Object wrap(Object view);
}
