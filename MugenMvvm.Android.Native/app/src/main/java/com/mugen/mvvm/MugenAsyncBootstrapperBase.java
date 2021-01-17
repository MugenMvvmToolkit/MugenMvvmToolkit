package com.mugen.mvvm;

import android.content.Context;
import android.content.pm.ProviderInfo;

import com.mugen.mvvm.interfaces.IAsyncAppInitializer;
import com.mugen.mvvm.interfaces.IHasAsyncInitializationCallback;
import com.mugen.mvvm.internal.AsyncAppInitializer;
import com.mugen.mvvm.views.ActivityMugenExtensions;

public abstract class MugenAsyncBootstrapperBase extends MugenBootstrapperBase implements Runnable {
    private static Thread _initThread;

    public MugenAsyncBootstrapperBase() {
        _initThread = new Thread(this);
        _initThread.start();
    }

    public static void ensureInitialized() {
        if (_initThread != null)
            waitInit();
    }

    @Override
    public void attachInfo(Context context, ProviderInfo info) {
        initializeNative(context, info);
    }

    @Override
    public void run() {
        if (_initThread == null) {
            IAsyncAppInitializer initializer = MugenService.getAsyncAppInitializer();
            MugenService.setAsyncAppInitializer(null);
            Context currentActivity = ActivityMugenExtensions.getCurrentActivity();
            boolean hasCallback = currentActivity instanceof IHasAsyncInitializationCallback;
            if (hasCallback)
                ((IHasAsyncInitializationCallback) currentActivity).onAsyncInitializationCompleting();
            initializer.onInitializationCompleted();
            if (hasCallback)
                ((IHasAsyncInitializationCallback) currentActivity).onAsyncInitializationCompleted();
        } else {
            initialize();
            _initThread = null;
            MugenUtils.runOnUiThread(this);
        }
    }

    @Override
    protected void initializeNative(Context context, ProviderInfo info) {
        super.initializeNative(context, info);
        AsyncAppInitializer initializer = new AsyncAppInitializer();
        MugenService.setAsyncAppInitializer(initializer);
        initializer.onInitializationStarted();
    }

    private static void waitInit() {
        Thread initThread = _initThread;
        if (initThread == null)
            return;
        try {
            initThread.join();
        } catch (InterruptedException ignored) {
        }
    }
}
