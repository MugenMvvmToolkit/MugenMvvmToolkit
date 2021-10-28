package com.mugen.mvvm;

import android.content.Context;
import android.content.pm.ProviderInfo;

import com.mugen.mvvm.interfaces.IAsyncAppInitializer;
import com.mugen.mvvm.interfaces.IHasAsyncInitializationCallback;
import com.mugen.mvvm.internal.AsyncAppInitializer;
import com.mugen.mvvm.views.ActivityMugenExtensions;

import java.util.concurrent.CountDownLatch;

public abstract class MugenAsyncBootstrapperBase extends MugenBootstrapperBase implements Runnable {
    private static Thread _initThread;
    private static CountDownLatch _mainThreadSignal;

    public MugenAsyncBootstrapperBase() {
        if (isAsync()) {
            _mainThreadSignal = new CountDownLatch(1);
            _initThread = new Thread(this);
            _initThread.start();
        }
    }

    public static void ensureInitialized() {
        if (_initThread != null)
            waitInit();
    }

    @Override
    public void attachInfo(Context context, ProviderInfo info) {
        if (initializeNative(context, info) && _initThread == null)
            initialize();
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
            try {
                _mainThreadSignal.await();
            } catch (InterruptedException ignored) {
            }
            initialize();
            _mainThreadSignal = null;
            _initThread = null;
            MugenUtils.runOnUiThread(this);
        }
    }

    @Override
    protected void initializeNativeInternal(Context context, ProviderInfo info) {
        super.initializeNativeInternal(context, info);
        if (_initThread != null) {
            AsyncAppInitializer initializer = new AsyncAppInitializer();
            MugenService.setAsyncAppInitializer(initializer);
            initializer.onInitializationStarted();
            _mainThreadSignal.countDown();
        }
    }

    protected boolean isAsync() {
        return true;
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
