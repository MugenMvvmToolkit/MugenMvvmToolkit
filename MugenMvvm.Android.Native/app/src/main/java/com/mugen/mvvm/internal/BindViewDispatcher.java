package com.mugen.mvvm.internal;

import android.content.Context;
import android.content.res.TypedArray;
import android.util.AttributeSet;
import android.view.View;
import com.mugen.mvvm.R;
import com.mugen.mvvm.interfaces.views.IBindViewCallback;
import com.mugen.mvvm.interfaces.views.IViewDispatcher;
import com.mugen.mvvm.views.ActivityExtensions;
import com.mugen.mvvm.views.AdapterViewExtensions;
import com.mugen.mvvm.views.ViewExtensions;
import com.mugen.mvvm.views.support.RecyclerViewExtensions;
import com.mugen.mvvm.views.support.ViewPager2Extensions;
import com.mugen.mvvm.views.support.ViewPagerExtensions;

public class BindViewDispatcher implements IViewDispatcher {
    private final static ViewAttributeAccessor _accessor = new ViewAttributeAccessor();
    private final IBindViewCallback _viewBindCallback;

    public BindViewDispatcher(IBindViewCallback viewBindCallback) {
        _viewBindCallback = viewBindCallback;
    }

    @Override
    public void onParentChanged(View view) {
        ViewExtensions.onMemberChanged(view, ViewExtensions.ParentMemberName, null);
        ViewExtensions.onMemberChanged(view, ViewExtensions.ParentEventName, null);
    }

    @Override
    public void onSetting(Object owner, View view) {
        _viewBindCallback.onSetView(ActivityExtensions.tryWrapActivity(owner), view);
    }

    @Override
    public void onSet(Object owner, View view) {
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
    public void onDestroy(View view) {
        if (ViewPagerExtensions.isSupported(view))
            ViewPagerExtensions.onDestroy(view);
        else if (ViewPager2Extensions.isSupported(view))
            ViewPager2Extensions.onDestroy(view);
        else if (RecyclerViewExtensions.isSupported(view))
            RecyclerViewExtensions.onDestroy(view);
        else if (AdapterViewExtensions.isSupported(view))
            AdapterViewExtensions.onDestroy(view);
        ViewExtensions.setAttachedValues(view, null);
    }

    @Override
    public View tryCreate(View parent, String name, Context viewContext, AttributeSet attrs) {
        return null;
    }
}
