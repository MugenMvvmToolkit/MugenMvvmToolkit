package com.mugen.mvvm;

import android.app.UiModeManager;
import android.content.Context;
import android.content.pm.PackageManager;
import android.content.res.Configuration;
import android.os.Build;

public final class DeviceService {
    public static final int Tv = 1;
    public static final int Desktop = 2;
    public static final int Watch = 3;
    public static final int Tablet = 4;
    public static final int Phone = 5;

    private static final int TabletCrossover = 600;

    private DeviceService() {

    }

    public static String appVersion() {
        Context appContext = MugenNativeService.getAppContext();
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
        Context appContext = MugenNativeService.getAppContext();
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
}
