package com.mugen.mvvm.views.listeners;

import androidx.viewpager2.widget.ViewPager2;

import com.mugen.mvvm.views.ViewMugenExtensions;

public class ViewPager2SelectedIndexListener extends ViewPager2.OnPageChangeCallback implements ViewMugenExtensions.IMemberListener {
    private final ViewPager2 _viewPager;
    private short _selectedIndexChangedCount;

    public ViewPager2SelectedIndexListener(Object viewPager) {
        _viewPager = (ViewPager2) viewPager;
    }

    @Override
    public void onPageSelected(int position) {
        ViewMugenExtensions.onMemberChanged(_viewPager, ViewMugenExtensions.SelectedIndexName, null);
        ViewMugenExtensions.onMemberChanged(_viewPager, ViewMugenExtensions.SelectedIndexEventName, null);
    }

    @Override
    public void addListener(Object target, String memberName) {
        if (ViewMugenExtensions.SelectedIndexName.equals(memberName) || ViewMugenExtensions.SelectedIndexEventName.equals(memberName) && _selectedIndexChangedCount++ == 0)
            _viewPager.registerOnPageChangeCallback(this);
    }

    @Override
    public void removeListener(Object target, String memberName) {
        if (ViewMugenExtensions.SelectedIndexName.equals(memberName) || ViewMugenExtensions.SelectedIndexEventName.equals(memberName) && _selectedIndexChangedCount != 0 && --_selectedIndexChangedCount == 0)
            _viewPager.unregisterOnPageChangeCallback(this);
    }
}
