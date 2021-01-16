package com.mugen.mvvm.internal;

import android.content.Context;
import android.content.res.TypedArray;
import android.util.AttributeSet;
import android.view.View;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.mugen.mvvm.R;
import com.mugen.mvvm.constants.PriorityConstants;
import com.mugen.mvvm.interfaces.views.IBindViewCallback;
import com.mugen.mvvm.interfaces.views.IViewDispatcher;
import com.mugen.mvvm.views.BindableMemberMugenExtensions;
import com.mugen.mvvm.views.ViewMugenExtensions;

public class BindViewDispatcher implements IViewDispatcher {
    private final static ViewAttributeAccessor _accessor = new ViewAttributeAccessor();
    private final IBindViewCallback _viewBindCallback;

    public BindViewDispatcher(IBindViewCallback viewBindCallback) {
        _viewBindCallback = viewBindCallback;
        viewBindCallback.setViewAccessor(_accessor);
    }

    @Override
    public void onParentChanged(@NonNull View view) {
        BindableMemberMugenExtensions.onMemberChanged(view, BindableMemberMugenExtensions.ParentMemberName, null);
        BindableMemberMugenExtensions.onMemberChanged(view, BindableMemberMugenExtensions.ParentEventName, null);
    }

    @Override
    public void onInitializing(@NonNull Object owner, @NonNull View view) {
        _viewBindCallback.onSetView(ViewMugenExtensions.tryWrap(owner), view);
    }

    @Override
    public void onInitialized(@NonNull Object owner, @NonNull View view) {
    }

    @Override
    public void onInflating(int resourceId, @NonNull Context context) {
    }

    @Override
    public void onInflated(@NonNull View view, int resourceId, @NonNull Context context) {
    }

    @Override
    public View onCreated(@NonNull View view, @NonNull Context context, @NonNull AttributeSet attrs) {
        ViewAttachedValues attachedValues = ViewMugenExtensions.getNativeAttachedValues(view, true);
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
    public void onDestroy(@NonNull View view) {
    }

    @Override
    public View tryCreate(@Nullable View parent, @NonNull String name, @NonNull Context viewContext, @NonNull AttributeSet attrs) {
        return null;
    }

    @Override
    public int getPriority() {
        return PriorityConstants.Default;
    }
}
