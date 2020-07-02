package com.mugen.mvvm.models;

import com.mugen.mvvm.interfaces.views.IReleasable;

import java.lang.ref.WeakReference;

public abstract class WeakTargetBase<T> implements IReleasable {
    private WeakReference _targetRef;

    public WeakTargetBase(T target) {
        _targetRef = new WeakReference(target);
    }

    public final void release() {
        T target = getTarget();
        _targetRef = null;
        if (target != null) {
            onReleased(target);
            onWeakReferenceRemoved();
        }
    }

    protected T getTarget() {
        WeakReference targetRef = _targetRef;
        if (targetRef == null)
            return null;
        Object o = _targetRef.get();
        if (o == null)
            onWeakReferenceRemoved();
        return (T) o;
    }

    protected void onReleased(T target) {
    }

    protected void onWeakReferenceRemoved() {
    }
}
