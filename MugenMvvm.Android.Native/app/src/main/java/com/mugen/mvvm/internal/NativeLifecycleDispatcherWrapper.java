package com.mugen.mvvm.internal;

import com.mugen.mvvm.interfaces.ILifecycleDispatcher;
import com.mugen.mvvm.views.ViewMugenExtensions;

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
        return _lifecycleDispatcher.onLifecycleChanging(ViewMugenExtensions.tryWrap(target), lifecycle, ViewMugenExtensions.tryWrap(state));
    }

    @Override
    public void onLifecycleChanged(Object target, int lifecycle, Object state) {
        _lifecycleDispatcher.onLifecycleChanged(ViewMugenExtensions.tryWrap(target), lifecycle, ViewMugenExtensions.tryWrap(state));
    }

    @Override
    public int getPriority() {
        return _lifecycleDispatcher.getPriority();
    }
}
