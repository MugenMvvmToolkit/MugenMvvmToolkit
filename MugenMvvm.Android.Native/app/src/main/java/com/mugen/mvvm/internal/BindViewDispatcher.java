package com.mugen.mvvm.internal;

import android.content.Context;
import android.content.res.TypedArray;
import android.util.AttributeSet;
import android.view.View;
import com.mugen.mvvm.R;
import com.mugen.mvvm.interfaces.views.IViewBindCallback;
import com.mugen.mvvm.interfaces.views.IViewDispatcher;
import com.mugen.mvvm.interfaces.views.IViewParentChangedCallback;

public class BindViewDispatcher implements IViewDispatcher {
    private final static ViewAttributeAccessor _accessor = new ViewAttributeAccessor();
    private final IViewBindCallback _viewBindCallback;
    private final IViewParentChangedCallback _parentChangedCallback;

    public BindViewDispatcher(IViewBindCallback viewBindCallback, IViewParentChangedCallback parentChangedCallback) {
        _viewBindCallback = viewBindCallback;
        _parentChangedCallback = parentChangedCallback;
    }

    @Override
    public void onParentChanged(View view) {
        if (_parentChangedCallback != null)
            _parentChangedCallback.onParentChanged(view);
    }

    @Override
    public void onSettingView(Object owner, View view) {
        _viewBindCallback.onSetView(owner, view);
    }

    @Override
    public void onSetView(Object owner, View view) {
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

        TypedArray typedArray = context.getTheme().obtainStyledAttributes(attrs, R.styleable.Bind, 0, 0);
        try {
            if (typedArray.getIndexCount() != 0) {
                _accessor.setTypedArray(typedArray);
                _viewBindCallback.bind(view, _accessor);
            }
        } finally {
            _accessor.setTypedArray(null);
            typedArray.recycle();
        }

        ViewParentObserver.Instance.add(view);
        return view;
    }

    @Override
    public View tryCreateCustomView(View parent, String name, Context viewContext, AttributeSet attrs) {
        return null;
    }
}
