package com.mugen.mvvm.views.listeners;

import com.mugen.mvvm.views.ViewExtensions;

class ViewMemberListenerUtils {
    public static ViewExtensions.IMemberListener getSwipeRefreshLayoutRefreshedListener(Object target){
        return new SwipeRefreshLayoutRefreshedListener(target);
    }

    public static ViewExtensions.IMemberListener getViewPagerSelectedIndexListener(Object target){
        return new ViewPagerSelectedIndexListener(target);
    }

    public static ViewExtensions.IMemberListener getViewPager2SelectedIndexListener(Object target){
        return new ViewPager2SelectedIndexListener(target);
    }

    public static ViewExtensions.IMemberListener getTabLayoutSelectedIndexListener(Object target){
        return new TabLayoutSelectedIndexListener(target);
    }
}
