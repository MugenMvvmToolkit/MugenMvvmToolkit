package com.mugen.mvvm;

import android.content.Context;
import com.mugen.mvvm.extensions.MugenExtensions;
import com.mugen.mvvm.interfaces.ILifecycleDispatcher;
import com.mugen.mvvm.interfaces.views.IActivityView;
import com.mugen.mvvm.interfaces.views.IViewBindCallback;
import com.mugen.mvvm.interfaces.views.IViewDispatcher;
import com.mugen.mvvm.internal.MugenService;
import com.mugen.mvvm.internal.NativeLifecycleDispatcherWrapper;
import com.mugen.mvvm.internal.NativeViewDispatcher;
import com.mugen.mvvm.internal.ViewWrapperCleanerLifecycleDispatcher;

public final class MugenNativeService {
    private MugenNativeService() {
    }

    public static void initialize(Context context, boolean includeSupportLibs) {
        MugenService.initialize(context, includeSupportLibs);
    }

    public static void nativeConfiguration(IViewBindCallback bindCallback) {
        MugenService.addViewDispatcher(new NativeViewDispatcher(bindCallback));
        MugenService.addLifecycleDispatcher(new ViewWrapperCleanerLifecycleDispatcher());
    }

    public static void addLifecycleDispatcher(ILifecycleDispatcher dispatcher, boolean wrap) {
        if (wrap)
            dispatcher = new NativeLifecycleDispatcherWrapper(dispatcher);
        MugenService.addLifecycleDispatcher(dispatcher);
    }

    public static void removeLifecycleDispatcher(ILifecycleDispatcher dispatcher) {
        MugenService.removeLifecycleDispatcher(dispatcher);
    }

    public static void addViewDispatcher(IViewDispatcher viewDispatcher) {
        MugenService.addViewDispatcher(viewDispatcher);
    }

    public static void removeViewDispatcher(IViewDispatcher viewDispatcher) {
        MugenService.removeViewDispatcher(viewDispatcher);
    }

    public static boolean startActivity(IActivityView activityView, Class activityClass, int resourceId, int flags) {
        return MugenExtensions.startActivity(activityView, activityClass, resourceId, flags);
    }
}
