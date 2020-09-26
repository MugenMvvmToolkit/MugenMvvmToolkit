package com.mugen.mvvm;

import android.content.Context;
import android.content.res.Resources;
import android.os.Build;

import com.mugen.mvvm.views.ActivityExtensions;

public class MugenNativeUtils {
    public static float getDensity() {
        return MugenNativeService.getAppContext().getResources().getDisplayMetrics().density;
    }

    public static float getScaledDensity() {
        return MugenNativeService.getAppContext().getResources().getDisplayMetrics().scaledDensity;
    }

    public static float getXdpi() {
        return MugenNativeService.getAppContext().getResources().getDisplayMetrics().xdpi;
    }

    @SuppressWarnings("deprecation")
    public static int getResourceColor(String name) {
        Context context = getCurrentContext();
        int resourceId = getResourceId(context, name, "color");
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M)
            return context.getResources().getColor(resourceId, context.getTheme());
        return context.getResources().getColor(resourceId);
    }

    public static float getResourceDimen(String name) {
        Context context = getCurrentContext();
        return context.getResources().getDimension(getResourceId(context, name, "dimen"));
    }

    public static boolean getResourceBool(String name) {
        Context context = getCurrentContext();
        return context.getResources().getBoolean(getResourceId(context, name, "bool"));
    }

    public static int getResourceLayout(String name) {
        return getResourceId(getCurrentContext(), name, "layout");
    }

    public static int getResourceId(String name) {
        return getResourceId(getCurrentContext(), name, "id");
    }

    public static int getResourceInteger(String name) {
        Context context = getCurrentContext();
        return context.getResources().getInteger(getResourceId(context, name, "integer"));
    }

    public static String getResourceString(String name) {
        Context context = getCurrentContext();
        return context.getResources().getString(getResourceId(context, name, "string"));
    }

    private static int getResourceId(Context context, String name, String type) {
        Resources resources = context.getResources();
        int identifier = resources.getIdentifier(name, type, context.getPackageName());
        if (identifier == 0)
            identifier = resources.getIdentifier(name, type, "android");
        return identifier;
    }

    private static Context getCurrentContext() {
        Context currentActivity = ActivityExtensions.getCurrentActivity();
        if (currentActivity != null)
            return currentActivity;
        return MugenNativeService.getAppContext();
    }
}
