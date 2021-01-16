package com.mugen.mvvm.internal;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.mugen.mvvm.interfaces.ILifecycleDispatcher;
import com.mugen.mvvm.views.ViewMugenExtensions;

public class NativeLifecycleDispatcherWrapper implements ILifecycleDispatcher {
    private final ILifecycleDispatcher _lifecycleDispatcher;

    public NativeLifecycleDispatcherWrapper(@NonNull ILifecycleDispatcher lifecycleDispatcher) {
        _lifecycleDispatcher = lifecycleDispatcher;
    }

    public ILifecycleDispatcher getNestedDispatcher() {
        return _lifecycleDispatcher;
    }

    @Override
    public boolean onLifecycleChanging(@NonNull Object target, int lifecycle, @Nullable Object state) {
        return _lifecycleDispatcher.onLifecycleChanging(ViewMugenExtensions.tryWrap(target), lifecycle, ViewMugenExtensions.tryWrap(state));
    }

    @Override
    public void onLifecycleChanged(@NonNull Object target, int lifecycle, @Nullable Object state) {
        _lifecycleDispatcher.onLifecycleChanged(ViewMugenExtensions.tryWrap(target), lifecycle, ViewMugenExtensions.tryWrap(state));
    }

    @Override
    public int getPriority() {
        return _lifecycleDispatcher.getPriority();
    }
}
