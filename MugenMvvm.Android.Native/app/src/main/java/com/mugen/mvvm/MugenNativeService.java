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
    private static boolean _compatSupported;
    private static boolean _materialSupported;
    private static boolean _rawViewTagMode;
    private static boolean _isNativeMode;

    private MugenNativeService() {
    }

    public static boolean isNativeMode() {
        return _isNativeMode;
    }

    public static boolean isCompatSupported() {
        return _compatSupported;
    }

    public static boolean isMaterialSupported() {
        return _materialSupported;
    }

    public static boolean isRawViewTagMode() {
        return _rawViewTagMode;
    }

    public static Context getAppContext() {
        return _context;
    }

    public static void initialize(Context context, IBindViewCallback bindCallback, boolean rawViewTagMode) {
        _context = context;
        _rawViewTagMode = rawViewTagMode;
        ViewCleaner viewCleaner = new ViewCleaner();
        FragmentDispatcher fragmentDispatcher = new FragmentDispatcher();
        ViewExtensions.addViewDispatcher(new BindViewDispatcher(bindCallback));
        ViewExtensions.addViewDispatcher(viewCleaner);
        ViewExtensions.addViewDispatcher(fragmentDispatcher);
        LifecycleExtensions.addLifecycleDispatcher(viewCleaner, false);
        LifecycleExtensions.addLifecycleDispatcher(fragmentDispatcher, false);
        LifecycleExtensions.addLifecycleDispatcher(new ActionBarHomeClickListener(), false);
        ViewExtensions.registerMemberListenerManager(new ViewMemberListenerManager());
    }

    public static void withSupportLibs(boolean compat, boolean material, boolean recyclerView, boolean swipeRefresh, boolean viewPager, boolean viewPager2) {
        if (compat)
            _compatSupported = true;
        if (material)
            _materialSupported = true;
        if (recyclerView)
            RecyclerViewExtensions.setSupported();
        if (swipeRefresh)
            SwipeRefreshLayoutExtensions.setSupported();
        if (viewPager)
            ViewPagerExtensions.setSupported();
        if (viewPager2)
            ViewPager2Extensions.setSupported();
    }

    public static void setNativeMode() {
        _isNativeMode = true;
    }
}
