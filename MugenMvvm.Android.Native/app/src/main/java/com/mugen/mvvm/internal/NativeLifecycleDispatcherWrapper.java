package com.mugen.mvvm.internal;

import com.mugen.mvvm.extensions.MugenExtensions;
import com.mugen.mvvm.interfaces.ILifecycleDispatcher;

public class NativeLifecycleDispatcherWrapper implements ILifecycleDispatcher {
    private final ILifecycleDispatcher _lifecycleDispatcher;

    public NativeLifecycleDispatcherWrapper(ILifecycleDispatcher lifecycleDispatcher) {
        _lifecycleDispatcher = lifecycleDispatcher;
    }

    public ILifecycleDispatcher getNestedDispatcher() {
        return _lifecycleDispatcher;
    }

    @Override
    public boolean onLifecycleChanging(Object target, int lifecycle, Object state) {
        return _lifecycleDispatcher.onLifecycleChanging(MugenExtensions.wrap(target, true), lifecycle, MugenExtensions.wrap(state, true));
    }

    @Override
    public void onLifecycleChanged(Object target, int lifecycle, Object state) {
        _lifecycleDispatcher.onLifecycleChanged(MugenExtensions.wrap(target, true), lifecycle, MugenExtensions.wrap(state, true));
    }
}
