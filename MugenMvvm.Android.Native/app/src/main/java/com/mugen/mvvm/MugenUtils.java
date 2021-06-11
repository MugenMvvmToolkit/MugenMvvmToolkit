package com.mugen.mvvm;

import android.annotation.SuppressLint;
import android.app.UiModeManager;
import android.content.Context;
import android.content.pm.PackageManager;
import android.content.res.Configuration;
import android.content.res.Resources;
import android.os.Build;
import android.os.Handler;

import androidx.annotation.NonNull;

import com.mugen.mvvm.constants.IdiomType;
import com.mugen.mvvm.constants.MugenInitializationFlags;
import com.mugen.mvvm.interfaces.IAsyncAppInitializer;
import com.mugen.mvvm.interfaces.ILifecycleDispatcher;
import com.mugen.mvvm.interfaces.views.IBindViewCallback;
import com.mugen.mvvm.internal.ActionBarHomeClickListener;
import com.mugen.mvvm.internal.ActivityTrackerDispatcher;
import com.mugen.mvvm.internal.BindViewDispatcher;
import com.mugen.mvvm.internal.FragmentDispatcher;
import com.mugen.mvvm.internal.ViewCleaner;
import com.mugen.mvvm.views.ActivityMugenExtensions;
import com.mugen.mvvm.views.listeners.ViewMemberListenerManager;

public final class MugenUtils {
    public static final String LogTag = "MugenMvvm";
    private static final int TabletCrossover = 600;

    @SuppressLint("StaticFieldLeak")
    private static Context _context;
    private static Handler _uiHandler;
    private static int _stateFlags;

    private MugenUtils() {
    }

    public static void ensureInitialized() {
        MugenAsyncBootstrapperBase.ensureInitialized();
    }

    public static boolean isFragmentStateDisabled() {
        return hasFlag(MugenInitializationFlags.NoFragmentState);
    }

    public static boolean isNativeMode() {
        return hasFlag(MugenInitializationFlags.NativeMode);
    }

    public static boolean isCompatSupported() {
        return hasFlag(MugenInitializationFlags.CompatLib);
    }

    public static boolean isRawViewTagMode() {
        return !hasFlag(MugenInitializationFlags.RawViewTagModeDisabled);
    }

    public static boolean isAsyncInitializing() {
        return MugenService.getAsyncAppInitializer() != null;
    }

    public static boolean hasFlag(int flag) {
        return (_stateFlags & flag) == flag;
    }

    public static void addFlag(int flag) {
        _stateFlags |= flag;
    }

    public static void removeFlag(int flag) {
        _stateFlags &= ~flag;
    }

    public static boolean isOnUiThread() {
        return _context.getMainLooper().getThread() == Thread.currentThread();
    }

    public static void runOnUiThread(@NonNull Runnable action) {
        if (isOnUiThread())
            action.run();
        else
            _uiHandler.post(action);
    }

    @NonNull
    public static Context getAppContext() {
        return _context;
    }

    public static void setAppContext(@NonNull Context context) {
        _context = context.getApplicationContext();
        _uiHandler = new Handler(_context.getMainLooper());
    }

    @NonNull
    public static Context getCurrentContext() {
        Context currentActivity = ActivityMugenExtensions.getCurrentActivity();
        if (currentActivity != null)
            return currentActivity;
        return MugenUtils.getAppContext();
    }

    public static void initializeCore(@NonNull Context context, int flags) {
        addFlag(flags);
        setAppContext(context);
        ViewCleaner viewCleaner = new ViewCleaner();
        FragmentDispatcher fragmentDispatcher = new FragmentDispatcher();
        MugenService.addViewDispatcher(viewCleaner);
        MugenService.addViewDispatcher(fragmentDispatcher);
        MugenService.addLifecycleDispatcher(viewCleaner, false);
        MugenService.addLifecycleDispatcher(fragmentDispatcher, false);
        MugenService.addLifecycleDispatcher(new ActionBarHomeClickListener(), false);
        MugenService.addLifecycleDispatcher(new ActivityTrackerDispatcher(), false);
        MugenService.addMemberListenerManager(new ViewMemberListenerManager());
    }

    public static void initialize(@NonNull IBindViewCallback bindCallback, @NonNull ILifecycleDispatcher lifecycleDispatcher) {
        IAsyncAppInitializer initializer = MugenService.getAsyncAppInitializer();
        if (initializer == null) {
            MugenService.addViewDispatcher(new BindViewDispatcher(bindCallback));
            MugenService.addLifecycleDispatcher(lifecycleDispatcher, isNativeMode());
        } else
            initializer.initialize(bindCallback, lifecycleDispatcher);
    }

    @NonNull
    public static String appVersion() {
        Context appContext = MugenUtils.getAppContext();
        try {
            return appContext.getPackageManager().getPackageInfo(appContext.getPackageName(), PackageManager.GET_META_DATA).versionName;
        } catch (Exception ignored) {
            return "0.0";
        }
    }

    @NonNull
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
                    return IdiomType.Tv;
                if (modeType == Configuration.UI_MODE_TYPE_DESK)
                    return IdiomType.Desktop;
                if (modeType == 0x06 /*Configuration.UI_MODE_TYPE_WATCH*/)
                    return IdiomType.Watch;
            }
        } catch (Exception ignored) {
        }

        if (appContext.getResources().getConfiguration().smallestScreenWidthDp >= TabletCrossover)
            return IdiomType.Tablet;
        return IdiomType.Phone;
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
