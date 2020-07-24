package com.mugen.mvvm;

import android.content.Context;
import com.mugen.mvvm.interfaces.views.*;
import com.mugen.mvvm.internal.*;
import com.mugen.mvvm.views.LifecycleExtensions;
import com.mugen.mvvm.views.ViewExtensions;
import com.mugen.mvvm.views.listeners.ViewMemberListenerManager;
import com.mugen.mvvm.views.support.*;

public final class MugenNativeService {
    private static Context _context;

    private MugenNativeService() {
    }

    public static Context getAppContext() {
        return _context;
    }

    public static void initialize(Context context, IBindViewCallback bindCallback) {
        _context = context;
        ViewExtensions.addViewDispatcher(new BindViewDispatcher(bindCallback));
        LifecycleExtensions.addLifecycleDispatcher(new FragmentStateCleaner(), false);
        LifecycleExtensions.addLifecycleDispatcher(new ViewCleaner(), false);
        ViewExtensions.registerMemberListenerManager(new ViewMemberListenerManager());
    }

    public static void withSupportLibs(boolean recyclerView, boolean toolbar, boolean swipeRefresh, boolean viewPager, boolean viewPager2) {
        if (recyclerView)
            RecyclerViewExtensions.setSupported();
        if (toolbar)
            ToolbarCompatExtensions.setSupported();
        if (swipeRefresh)
            SwipeRefreshLayoutExtensions.setSupported();
        if (viewPager)
            ViewPagerExtensions.setSupported();
        if (viewPager2)
            ViewPager2Extensions.setSupported();
    }
}
