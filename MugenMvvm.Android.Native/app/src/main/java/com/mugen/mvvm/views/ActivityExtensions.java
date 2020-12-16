package com.mugen.mvvm.views;

import android.annotation.SuppressLint;
import android.app.Activity;
import android.content.Context;
import android.content.ContextWrapper;
import android.content.Intent;
import android.view.View;
import android.widget.Toolbar;

import androidx.appcompat.app.AppCompatActivity;

import com.mugen.mvvm.MugenNativeService;
import com.mugen.mvvm.interfaces.views.IActivityView;
import com.mugen.mvvm.views.activities.MainMugenActivity;
import com.mugen.mvvm.views.activities.MainMugenAppCompatActivity;
import com.mugen.mvvm.views.activities.MugenActivity;
import com.mugen.mvvm.views.activities.MugenAppCompatActivity;

public final class ActivityExtensions {
    public static final String ViewIdIntentKey = "~v_id!";
    public static final String ViewModelIdIntentKey = "~vm_id!";
    public static final String RequestIdIntentKey = "~r_id!";

    @SuppressLint("StaticFieldLeak")
    private static Activity _currentActivity;

    private ActivityExtensions() {
    }

    public static boolean isTaskRoot(IActivityView activityView) {
        Activity activity = (Activity) activityView.getActivity();
        if (activity.isTaskRoot())
            return true;
        return false;
    }

    public static int getRequestId(IActivityView activityView) {
        Activity activity = (Activity) activityView.getActivity();
        return activity.getIntent().getIntExtra(RequestIdIntentKey, 0);
    }

    public static String getViewModelId(IActivityView activityView) {
        Activity activity = (Activity) activityView.getActivity();
        return activity.getIntent().getStringExtra(ViewModelIdIntentKey);
    }

    public static Object getActionBar(IActivityView activityView) {
        Activity activity = (Activity) activityView.getActivity();
        if (MugenNativeService.isCompatSupported() && activity instanceof AppCompatActivity)
            return ((AppCompatActivity) activity).getSupportActionBar();
        return activity.getActionBar();
    }

    @SuppressLint("NewApi")
    public static boolean setActionBar(IActivityView activityView, View toolbar) {
        if (ToolbarExtensions.isSupportedCompat(toolbar)) {
            AppCompatActivity activity = (AppCompatActivity) activityView.getActivity();
            activity.setSupportActionBar((androidx.appcompat.widget.Toolbar) toolbar);
            return true;
        }
        if (ToolbarExtensions.isSupported(toolbar)) {
            Activity activity = (Activity) activityView.getActivity();
            activity.setActionBar((Toolbar) toolbar);
            return true;
        }
        return false;
    }

    public static boolean startActivity(IActivityView activityView, Class activityClass, int requestId, String viewModelId, int resourceId, int flags) {
        if (activityClass == null)
            activityClass = ViewExtensions.tryGetClassById(resourceId);
        if (activityClass == null)
            return false;

        Context context = activityView == null ? MugenNativeService.getAppContext() : activityView.getActivity();
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

    public static Context getActivity(Context context) {
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

    public static void setMainActivityMapping(int resource, boolean isCompat) {
        ViewExtensions.addViewMapping(isCompat ? MainMugenAppCompatActivity.class : MainMugenActivity.class, resource);
    }

    public static void addCommonActivityMapping(int resource, boolean isCompat) {
        ViewExtensions.addViewMapping(isCompat ? MugenAppCompatActivity.class : MugenActivity.class, resource);
    }


    public static Context getCurrentActivity() {
        return _currentActivity;
    }

    public static void setCurrentActivity(Context activity) {
        _currentActivity = (Activity) activity;
    }

    public static void clearCurrentActivity(Context activity) {
        if (_currentActivity != null && _currentActivity.equals(activity))
            _currentActivity = null;
    }
}
