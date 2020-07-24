package com.mugen.mvvm.internal;

import com.mugen.mvvm.interfaces.ILifecycleDispatcher;
import com.mugen.mvvm.views.ActivityExtensions;

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
        return _lifecycleDispatcher.onLifecycleChanging(ActivityExtensions.tryWrapActivity(target), lifecycle, ActivityExtensions.tryWrapActivity(state));
    }

    @Override
    public void onLifecycleChanged(Object target, int lifecycle, Object state) {
        _lifecycleDispatcher.onLifecycleChanged(ActivityExtensions.tryWrapActivity(target), lifecycle, ActivityExtensions.tryWrapActivity(state));
    }
}
