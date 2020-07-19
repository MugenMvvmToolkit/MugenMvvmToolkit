package com.mugen.mvvm.interfaces.views;

import android.content.Context;
import android.util.AttributeSet;
import android.view.View;

public interface IViewDispatcher {
    void onParentChanged(View view);

    void onSettingView(Object owner, View view);

    void onSetView(Object owner, View view);

    void onInflatingView(int resourceId, Context context);

    void onInflatedView(View view, int resourceId, Context context);

    View onViewCreated(View view, Context context, AttributeSet attrs);

    View tryCreateCustomView(View parent, String name, Context viewContext, AttributeSet attrs);
}
