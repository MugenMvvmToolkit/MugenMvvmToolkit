package com.mugen.mvvm.interfaces.views;

import android.content.Context;
import android.util.AttributeSet;
import android.view.View;

import com.mugen.mvvm.interfaces.IHasPriority;

public interface IViewDispatcher extends IHasPriority {
    void onParentChanged(View view);

    void onSetting(Object owner, View view);

    void onSet(Object owner, View view);

    void onInflating(int resourceId, Context context);

    void onInflated(View view, int resourceId, Context context);

    View onCreated(View view, Context context, AttributeSet attrs);

    void onDestroy(View view);

    View tryCreate(View parent, String name, Context viewContext, AttributeSet attrs);
}
