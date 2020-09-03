package com.mugen.mvvm;

import android.content.Context;
import android.content.res.Resources;
import android.os.Build;
import android.util.TypedValue;
import com.mugen.mvvm.views.ActivityExtensions;

public class MugenNativeUtils {
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

    public static float dpToPx(float value) {
        return TypedValue.applyDimension(TypedValue.COMPLEX_UNIT_DIP, value, MugenNativeService.getAppContext().getResources().getDisplayMetrics());
    }

    public static float spToPx(float value) {
        return TypedValue.applyDimension(TypedValue.COMPLEX_UNIT_SP, value, MugenNativeService.getAppContext().getResources().getDisplayMetrics());
    }

    public static float ptToPx(float value) {
        return TypedValue.applyDimension(TypedValue.COMPLEX_UNIT_PT, value, MugenNativeService.getAppContext().getResources().getDisplayMetrics());
    }

    public static float inToPx(float value) {
        return TypedValue.applyDimension(TypedValue.COMPLEX_UNIT_IN, value, MugenNativeService.getAppContext().getResources().getDisplayMetrics());
    }

    public static float mmToPx(float value) {
        return TypedValue.applyDimension(TypedValue.COMPLEX_UNIT_MM, value, MugenNativeService.getAppContext().getResources().getDisplayMetrics());
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
