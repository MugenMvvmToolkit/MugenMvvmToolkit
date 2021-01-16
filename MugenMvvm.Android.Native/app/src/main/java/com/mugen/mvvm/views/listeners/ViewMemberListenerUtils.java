package com.mugen.mvvm.views.listeners;

import com.mugen.mvvm.interfaces.IMemberListener;

class ViewMemberListenerUtils {
    public static IMemberListener getSwipeRefreshLayoutRefreshedListener(Object target) {
        return new SwipeRefreshLayoutRefreshedListener(target);
    }

    public static IMemberListener getViewPagerSelectedIndexListener(Object target) {
        return new ViewPagerSelectedIndexListener(target);
    }

    public static IMemberListener getViewPager2SelectedIndexListener(Object target) {
        return new ViewPager2SelectedIndexListener(target);
    }

    public static IMemberListener getTabLayoutSelectedIndexListener(Object target) {
        return new TabLayoutSelectedIndexListener(target);
    }
}
