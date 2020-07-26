package com.mugen.mvvm.views;

import com.mugen.mvvm.interfaces.ILifecycleDispatcher;
import com.mugen.mvvm.internal.HasPriorityComparator;
import com.mugen.mvvm.internal.NativeLifecycleDispatcherWrapper;

import java.util.ArrayList;
import java.util.Collection;
import java.util.Collections;

public final class LifecycleExtensions {
    private final static ArrayList<ILifecycleDispatcher> _lifecycleDispatchers = new ArrayList<>();

    private LifecycleExtensions() {
    }

    public static void addLifecycleDispatcher(ILifecycleDispatcher dispatcher, boolean wrap) {
        if (wrap)
            dispatcher = new NativeLifecycleDispatcherWrapper(dispatcher);
        _lifecycleDispatchers.add(dispatcher);
        Collections.sort(_lifecycleDispatchers, HasPriorityComparator.Instance);
    }

    public static void removeLifecycleDispatcher(ILifecycleDispatcher dispatcher) {
        if (_lifecycleDispatchers.remove(dispatcher))
            return;
        for (int i = 0; i < _lifecycleDispatchers.size(); i++) {
            ILifecycleDispatcher d = _lifecycleDispatchers.get(i);
            if (d instanceof NativeLifecycleDispatcherWrapper && ((NativeLifecycleDispatcherWrapper) d).getNestedDispatcher().equals(dispatcher)) {
                _lifecycleDispatchers.remove(i);
                return;
            }
        }
    }

    public static boolean onLifecycleChanging(Object target, int lifecycle, Object state) {
        boolean result = true;
        for (int i = 0; i < _lifecycleDispatchers.size(); i++) {
            if (!_lifecycleDispatchers.get(i).onLifecycleChanging(target, lifecycle, state))
                result = false;
        }
        return result;
    }

    public static void onLifecycleChanged(Object target, int lifecycle, Object state) {
        for (int i = 0; i < _lifecycleDispatchers.size(); i++) {
            _lifecycleDispatchers.get(i).onLifecycleChanged(target, lifecycle, state);
        }
    }
}
