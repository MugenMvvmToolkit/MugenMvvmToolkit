package com.mugen.mvvm;

import android.annotation.SuppressLint;
import android.app.UiModeManager;
import android.content.Context;
import android.content.pm.PackageManager;
import android.content.res.Configuration;
import android.content.res.Resources;
import android.os.Build;

import com.mugen.mvvm.constants.MugenInitializationFlags;
import com.mugen.mvvm.interfaces.views.IBindViewCallback;
import com.mugen.mvvm.internal.ActionBarHomeClickListener;
import com.mugen.mvvm.internal.ActivityTrackerDispatcher;
import com.mugen.mvvm.internal.BindViewDispatcher;
import com.mugen.mvvm.internal.FragmentDispatcher;
import com.mugen.mvvm.internal.ViewCleaner;
import com.mugen.mvvm.views.ActivityMugenExtensions;
import com.mugen.mvvm.views.LifecycleMugenExtensions;
import com.mugen.mvvm.views.ViewMugenExtensions;
import com.mugen.mvvm.views.listeners.ViewMemberListenerManager;
import com.mugen.mvvm.views.support.RecyclerViewMugenExtensions;
import com.mugen.mvvm.views.support.SwipeRefreshLayoutMugenExtensions;
import com.mugen.mvvm.views.support.ViewPager2MugenExtensions;
import com.mugen.mvvm.views.support.ViewPagerMugenExtensions;

public final class MugenUtils {
    public static final int Tv = 1;
    public static final int Desktop = 2;
    public static final int Watch = 3;
    public static final int Tablet = 4;
    public static final int Phone = 5;

    private static final int TabletCrossover = 600;

    @SuppressLint("StaticFieldLeak")
    private static Context _context;
    private static int _stateFlags;

    private MugenUtils() {
    }

    public static boolean isFragmentStateDisabled() {
        return hasFlag(MugenInitializationFlags.FragmentStateDisabled);
    }

    public static boolean isNativeMode() {
        return hasFlag(MugenInitializationFlags.NativeMode);
    }

    public static boolean isCompatSupported() {
        return hasFlag(MugenInitializationFlags.CompatLib);
    }

    public static boolean isMaterialSupported() {
        return hasFlag(MugenInitializationFlags.MaterialLib);
    }

    public static boolean isRawViewTagMode() {
        return !hasFlag(MugenInitializationFlags.RawViewTagModeDisabled);
    }

    public static boolean hasFlag(int flag) {
        return (_stateFlags & flag) == flag;
    }

    public static Context getAppContext() {
        return _context;
    }

    public static void setAppContext(Context context) {
        _context = context.getApplicationContext();
    }

    public static Context getCurrentContext() {
        Context currentActivity = ActivityMugenExtensions.getCurrentActivity();
        if (currentActivity != null)
            return currentActivity;
        return MugenUtils.getAppContext();
    }

    public static void initializeCore(Context context) {
        setAppContext(context);
        ViewCleaner viewCleaner = new ViewCleaner();
        FragmentDispatcher fragmentDispatcher = new FragmentDispatcher();
        ViewMugenExtensions.addViewDispatcher(viewCleaner);
        ViewMugenExtensions.addViewDispatcher(fragmentDispatcher);
        LifecycleMugenExtensions.addLifecycleDispatcher(viewCleaner, false);
        LifecycleMugenExtensions.addLifecycleDispatcher(fragmentDispatcher, false);
        LifecycleMugenExtensions.addLifecycleDispatcher(new ActionBarHomeClickListener(), false);
        LifecycleMugenExtensions.addLifecycleDispatcher(new ActivityTrackerDispatcher(), false);
        ViewMugenExtensions.registerMemberListenerManager(new ViewMemberListenerManager());
    }

    public static void initialize(IBindViewCallback bindCallback, int stateFlags) {
        _stateFlags = stateFlags;
        ViewMugenExtensions.addViewDispatcher(new BindViewDispatcher(bindCallback));
        if (hasFlag(MugenInitializationFlags.RecyclerViewLib))
            RecyclerViewMugenExtensions.setSupported();
        if (hasFlag(MugenInitializationFlags.SwipeRefreshLib))
            SwipeRefreshLayoutMugenExtensions.setSupported();
        if (hasFlag(MugenInitializationFlags.ViewPagerLib))
            ViewPagerMugenExtensions.setSupported();
        if (hasFlag(MugenInitializationFlags.ViewPager2Lib))
            ViewPager2MugenExtensions.setSupported();
    }

    public static String appVersion() {
        Context appContext = MugenUtils.getAppContext();
        try {
            return appContext.getPackageManager().getPackageInfo(appContext.getPackageName(), PackageManager.GET_META_DATA).versionName;
        } catch (Exception ignored) {
            return "0.0";
        }
    }

    public static String version() {
        return Build.VERSION.RELEASE;
    }

    public static int idiom() {
        Context appContext = MugenUtils.getAppContext();
        try {
            UiModeManager modeManager = (UiModeManager) appContext.getSystemService(Context.UI_MODE_SERVICE);
            if (modeManager != null) {
                int modeType = modeManager.getCurrentModeType();
                if (modeType == Configuration.UI_MODE_TYPE_TELEVISION)
                    return Tv;
                if (modeType == Configuration.UI_MODE_TYPE_DESK)
                    return Desktop;
                if (modeType == 0x06 /*Configuration.UI_MODE_TYPE_WATCH*/)
                    return Watch;
            }
        } catch (Exception ignored) {
        }

        if (appContext.getResources().getConfiguration().smallestScreenWidthDp >= TabletCrossover)
            return Tablet;
        return Phone;
    }

    public static float getDensity() {
        return MugenUtils.getAppContext().getResources().getDisplayMetrics().density;
    }

    public static float getScaledDensity() {
        return MugenUtils.getAppContext().getResources().getDisplayMetrics().scaledDensity;
    }

    public static float getXdpi() {
        return MugenUtils.getAppContext().getResources().getDisplayMetrics().xdpi;
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
}
