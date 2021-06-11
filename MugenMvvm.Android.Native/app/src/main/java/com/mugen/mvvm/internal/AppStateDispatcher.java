package com.mugen.mvvm.internal;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.mugen.mvvm.MugenService;
import com.mugen.mvvm.constants.LifecycleState;
import com.mugen.mvvm.constants.PriorityConstant;
import com.mugen.mvvm.interfaces.ILifecycleDispatcher;

public class AppStateDispatcher implements ILifecycleDispatcher {
    private final Class _rootActivityClass;

    public AppStateDispatcher(@Nullable String rootActivityClass) {
        try {
            _rootActivityClass = rootActivityClass == null ? null : Class.forName(rootActivityClass);
        } catch (ClassNotFoundException e) {
            throw new RuntimeException(e);
        }
    }

    @Override
    public boolean onLifecycleChanging(@NonNull Object target, int lifecycle, @Nullable Object state) {
        if (lifecycle == LifecycleState.Create && target instanceof Activity) {
            MugenService.removeLifecycleDispatcher(this);
            if (state instanceof Bundle)
                return handle((Activity) target);
        }

        return true;
    }

    @Override
    public void onLifecycleChanged(@NonNull Object target, int lifecycle, @Nullable Object state) {
    }

    @Override
    public int getPriority() {
        return PriorityConstant.AppStateDispatcher;
    }

    private boolean handle(Activity activity) {
        if (_rootActivityClass == null) {
            if (Intent.ACTION_MAIN.equals(activity.getIntent().getAction()))
                return true;
        } else if (_rootActivityClass.equals(activity.getClass()))
            return true;

        Intent intent = getRootIntent(activity);
        intent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TASK);
        activity.finish();
        activity.startActivity(intent);
        return false;
    }

    private Intent getRootIntent(Activity activity) {
        if (_rootActivityClass == null)
            return activity.getPackageManager().getLaunchIntentForPackage(activity.getPackageName());
        return new Intent(activity, _rootActivityClass);
    }
}
