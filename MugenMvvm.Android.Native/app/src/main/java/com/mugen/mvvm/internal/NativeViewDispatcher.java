package com.mugen.mvvm.internal;

import android.content.Context;
import android.content.res.TypedArray;
import android.util.AttributeSet;
import android.view.View;
import com.mugen.mvvm.R;
import com.mugen.mvvm.extensions.MugenExtensions;
import com.mugen.mvvm.interfaces.views.IAndroidView;
import com.mugen.mvvm.interfaces.views.IViewBindCallback;
import com.mugen.mvvm.interfaces.views.IViewDispatcher;
import com.mugen.mvvm.views.ViewWrapper;

public class NativeViewDispatcher implements IViewDispatcher {

    private final static ViewAttributeAccessor _accessor = new ViewAttributeAccessor();
    private final IViewBindCallback _viewBindCallback;

    public NativeViewDispatcher(IViewBindCallback viewBindCallback) {
        _viewBindCallback = viewBindCallback;
    }

    @Override
    public void onParentChanged(View view) {
        IAndroidView wrapper = MugenExtensions.wrap(view, false);
        if (wrapper != null)
            wrapper.onMemberChanged(ViewWrapper.ParentMemberName, null);
    }

    @Override
    public void onSetView(Object owner, View view) {
        MugenExtensions.wrap(view, true).setParent(owner);
    }

    @Override
    public void onInflatingView(int resourceId, Context context) {
    }

    @Override
    public void onInflatedView(View view, int resourceId, Context context) {
    }

    @Override
    public View onViewCreated(View view, Context context, AttributeSet attrs) {
        if (view.getTag(R.id.bindHandled) != null)
            return view;

        view.setTag(R.id.bindHandled, "");
        boolean disableParentObserver = false;
        TypedArray typedArray = context.getTheme().obtainStyledAttributes(attrs, R.styleable.Bind, 0, 0);
        try {
            if (typedArray.getIndexCount() != 0) {
                if (typedArray.getBoolean(R.styleable.Bind_disableParentObserver, false)) {
                    disableParentObserver = true;
                    view.setTag(R.id.disableParentObserver, "");
                }
                _accessor.setTypedArray(typedArray);
                _viewBindCallback.bind(MugenExtensions.wrap(view, true), _accessor);
            }
        } finally {
            _accessor.setTypedArray(null);
            typedArray.recycle();
        }

        if (!disableParentObserver)
            ViewParentObserver.Instance.add(view);
        return view;
    }

    @Override
    public View tryCreateCustomView(View parent, String name, Context viewContext, AttributeSet attrs) {
        return null;
    }
}
