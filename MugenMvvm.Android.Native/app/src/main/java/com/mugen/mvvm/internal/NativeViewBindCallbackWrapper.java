package com.mugen.mvvm.internal;

import com.mugen.mvvm.extensions.MugenExtensions;
import com.mugen.mvvm.interfaces.views.IAndroidView;
import com.mugen.mvvm.interfaces.views.IViewAttributeAccessor;
import com.mugen.mvvm.interfaces.views.IViewBindCallback;
import com.mugen.mvvm.interfaces.views.IViewParentChangedCallback;
import com.mugen.mvvm.views.ViewWrapper;

public class NativeViewBindCallbackWrapper implements IViewBindCallback, IViewParentChangedCallback {
    private final IViewBindCallback _target;

    public NativeViewBindCallbackWrapper(IViewBindCallback target) {
        _target = target;
    }

    @Override
    public void onSetView(Object owner, Object view) {
        _target.onSetView(MugenExtensions.wrap(owner, true), MugenExtensions.wrap(view, true));
    }

    @Override
    public void bind(Object view, IViewAttributeAccessor bindAttrs) {
        _target.bind(MugenExtensions.wrap(view, true), bindAttrs);
    }

    @Override
    public void onParentChanged(Object view) {
        Object wrapper = MugenExtensions.wrap(view, false);
        if (wrapper instanceof IAndroidView)
            ((IAndroidView) wrapper).onMemberChanged(ViewWrapper.ParentMemberName, null);
    }
}
