package com.mugen.mvvm.interfaces.views;

import android.content.Context;
import android.util.AttributeSet;
import android.view.View;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.mugen.mvvm.interfaces.IHasPriority;

public interface IViewDispatcher extends IHasPriority {
    void onParentChanged(@NonNull View view);

    void onInitializing(@NonNull Object owner, @NonNull View view);

    void onInitialized(@NonNull Object owner, @NonNull View view);

    void onInflating(int resourceId, @NonNull Context context);

    void onInflated(@NonNull View view, int resourceId, @NonNull Context context);

    View onCreated(@NonNull View view, @NonNull Context context, @NonNull AttributeSet attrs);

    void onDestroy(@NonNull View view);

    View tryCreate(@Nullable View parent, @NonNull String name, @NonNull Context viewContext, @NonNull AttributeSet attrs);
}
