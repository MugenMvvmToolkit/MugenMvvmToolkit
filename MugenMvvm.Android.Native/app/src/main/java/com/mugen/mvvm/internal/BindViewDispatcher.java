package com.mugen.mvvm.internal;

import android.content.Context;
import android.content.res.TypedArray;
import android.util.AttributeSet;
import android.view.View;

import com.mugen.mvvm.R;
import com.mugen.mvvm.constants.PriorityConstants;
import com.mugen.mvvm.interfaces.views.IBindViewCallback;
import com.mugen.mvvm.interfaces.views.IViewDispatcher;
import com.mugen.mvvm.views.ViewExtensions;

public class BindViewDispatcher implements IViewDispatcher {
    private final static ViewAttributeAccessor _accessor = new ViewAttributeAccessor();
    private final IBindViewCallback _viewBindCallback;

    public BindViewDispatcher(IBindViewCallback viewBindCallback) {
        _viewBindCallback = viewBindCallback;
        viewBindCallback.setViewAccessor(_accessor);
    }

    @Override
    public void onParentChanged(View view) {
        ViewExtensions.onMemberChanged(view, ViewExtensions.ParentMemberName, null);
        ViewExtensions.onMemberChanged(view, ViewExtensions.ParentEventName, null);
    }

    @Override
    public void onInitializing(Object owner, View view) {
        _viewBindCallback.onSetView(ViewExtensions.tryWrap(owner), view);
    }

    @Override
    public void onInitialized(Object owner, View view) {
    }

    @Override
    public void onInflating(int resourceId, Context context) {
    }

    @Override
    public void onInflated(View view, int resourceId, Context context) {
    }

    @Override
    public View onCreated(View view, Context context, AttributeSet attrs) {
        ViewAttachedValues attachedValues = ViewExtensions.getNativeAttachedValues(view, true);
        if (attachedValues.isBindHandled())
            return view;
        attachedValues.setBindHandled(true);

        TypedArray typedArray = context.getTheme().obtainStyledAttributes(attrs, R.styleable.Bind, 0, 0);
        try {
            if (typedArray.getIndexCount() != 0) {
                _accessor.setTypedArray(typedArray);
                _viewBindCallback.bind(view);
            }
        } finally {
            _accessor.setTypedArray(null);
            typedArray.recycle();
        }

        ViewParentObserver.Instance.add(view);
        return view;
    }

    @Override
    public void onDestroy(View view) {
    }

    @Override
    public View tryCreate(View parent, String name, Context viewContext, AttributeSet attrs) {
        return null;
    }

    @Override
    public int getPriority() {
        return PriorityConstants.Default;
    }
}
