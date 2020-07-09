package com.mugen.mvvm.models;

import com.mugen.mvvm.interfaces.views.IReleasable;
import com.mugen.mvvm.internal.MugenService;

import java.lang.ref.WeakReference;

public abstract class WeakTargetBase<T> implements IReleasable {
    private WeakReference<T> _targetRef;

    public WeakTargetBase(T target) {
        _targetRef = new WeakReference<>(target);
    }

    public final void release() {
        T target = getTarget();
        if (_targetRef != null) {
            _targetRef = null;
            if (target != null)
                onReleased(target);
            MugenService.onWeakReferenceRemoved(this);
        }
    }

    protected T getTarget() {
        WeakReference<T> targetRef = _targetRef;
        if (targetRef == null)
            return null;
        T o = _targetRef.get();
        if (o == null) {
            _targetRef = null;
            MugenService.onWeakReferenceRemoved(this);
        }
        return o;
    }

    protected void onReleased(T target) {
    }
}
