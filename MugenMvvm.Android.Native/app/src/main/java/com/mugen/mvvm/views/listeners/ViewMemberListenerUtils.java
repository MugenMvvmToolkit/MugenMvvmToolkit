package com.mugen.mvvm.views.listeners;

import com.mugen.mvvm.views.ViewMugenExtensions;

class ViewMemberListenerUtils {
    public static ViewMugenExtensions.IMemberListener getSwipeRefreshLayoutRefreshedListener(Object target) {
        return new SwipeRefreshLayoutRefreshedListener(target);
    }

    public static ViewMugenExtensions.IMemberListener getViewPagerSelectedIndexListener(Object target) {
        return new ViewPagerSelectedIndexListener(target);
    }

    public static ViewMugenExtensions.IMemberListener getViewPager2SelectedIndexListener(Object target) {
        return new ViewPager2SelectedIndexListener(target);
    }

    public static ViewMugenExtensions.IMemberListener getTabLayoutSelectedIndexListener(Object target) {
        return new TabLayoutSelectedIndexListener(target);
    }
}
