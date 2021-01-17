package com.mugen.mvvm.interfaces;

import androidx.annotation.NonNull;

import com.mugen.mvvm.interfaces.views.IBindViewCallback;

public interface IAsyncAppInitializer {
    void initialize(@NonNull IBindViewCallback bindCallback, @NonNull ILifecycleDispatcher lifecycleDispatcher);

    void onInitializationStarted();

    void onInitializationCompleted();
}
