package com.mugen.mvvm.views;

import android.annotation.SuppressLint;
import android.app.Activity;
import android.content.Context;
import android.content.ContextWrapper;
import android.content.Intent;
import android.view.View;
import android.widget.Toolbar;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.appcompat.app.AppCompatActivity;

import com.mugen.mvvm.MugenService;
import com.mugen.mvvm.MugenUtils;
import com.mugen.mvvm.interfaces.IActivityManager;
import com.mugen.mvvm.interfaces.views.IActivityView;

public final class ActivityMugenExtensions {
    public static final String ViewIdIntentKey = "~v_id!";
    public static final String ViewModelIdIntentKey = "~vm_id!";
    public static final String RequestIdIntentKey = "~r_id!";

    @SuppressLint("StaticFieldLeak")
    private static Activity _currentActivity;

    private ActivityMugenExtensions() {
    }

    public static boolean isTaskRoot(@NonNull IActivityView activityView) {
        Activity activity = (Activity) activityView.getActivity();
        if (activity.isTaskRoot())
            return true;
        return false;
    }

    public static int getRequestId(@NonNull IActivityView activityView) {
        Activity activity = (Activity) activityView.getActivity();
        return activity.getIntent().getIntExtra(RequestIdIntentKey, 0);
    }

    public static String getViewModelId(@NonNull IActivityView activityView) {
        Activity activity = (Activity) activityView.getActivity();
        return activity.getIntent().getStringExtra(ViewModelIdIntentKey);
    }

    public static Object getActionBar(@NonNull IActivityView activityView) {
        Activity activity = (Activity) activityView.getActivity();
        if (MugenUtils.isCompatSupported() && activity instanceof AppCompatActivity)
            return ((AppCompatActivity) activity).getSupportActionBar();
        return activity.getActionBar();
    }

    @SuppressLint("NewApi")
    public static boolean setActionBar(@NonNull IActivityView activityView, View toolbar) {
        if (ToolbarMugenExtensions.isSupportedCompat(toolbar)) {
            AppCompatActivity activity = (AppCompatActivity) activityView.getActivity();
            activity.setSupportActionBar((androidx.appcompat.widget.Toolbar) toolbar);
            return true;
        }
        if (ToolbarMugenExtensions.isSupported(toolbar)) {
            Activity activity = (Activity) activityView.getActivity();
            activity.setActionBar((Toolbar) toolbar);
            return true;
        }
        return false;
    }

    public static boolean startActivity(@Nullable IActivityView activityView, @Nullable Class activityClass, int requestId, @Nullable String viewModelId, int resourceId, int flags) {
        IActivityManager activityManager = MugenService.getActivityManager();
        if (activityManager != null && activityManager.tryStartActivity(activityView, activityClass, requestId, viewModelId, resourceId, flags))
            return true;
        if (activityClass == null)
            activityClass = ViewMugenExtensions.tryGetClassByLayoutId(resourceId);
        if (activityClass == null)
            return false;

        Context context = activityView == null ? MugenUtils.getAppContext() : (Context) activityView.getActivity();
        Intent intent = new Intent(context, activityClass);
        if (activityView == null)
            intent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
        if (flags != 0)
            intent.addFlags(flags);
        if (resourceId != 0)
            intent.putExtra(ViewIdIntentKey, resourceId);
        if (viewModelId != null)
            intent.putExtra(ViewModelIdIntentKey, viewModelId);
        intent.putExtra(RequestIdIntentKey, requestId);
        context.startActivity(intent);
        return true;
    }

    @Nullable
    public static Context tryGetActivity(@NonNull Context context) {
        while (true) {
            if (context instanceof Activity)
                return context;

            if (context instanceof ContextWrapper) {
                context = ((ContextWrapper) context).getBaseContext();
                continue;
            }
            return null;
        }
    }

    @Nullable
    public static Context getCurrentActivity() {
        return _currentActivity;
    }

    public static void setCurrentActivity(@Nullable Context activity) {
        _currentActivity = (Activity) activity;
    }

    public static void clearCurrentActivity(@Nullable Context activity) {
        if (_currentActivity != null && _currentActivity.equals(activity))
            _currentActivity = null;
    }
}
