package com.mugen.mvvm.views.listeners;

import androidx.viewpager2.widget.ViewPager2;
import com.mugen.mvvm.views.ViewExtensions;

public class ViewPager2SelectedIndexListener extends ViewPager2.OnPageChangeCallback implements ViewExtensions.IMemberListener {
    private final ViewPager2 _viewPager;
    private short _selectedIndexChangedCount;

    public ViewPager2SelectedIndexListener(ViewPager2 viewPager) {
        _viewPager = viewPager;
    }

    @Override
    public void onPageSelected(int position) {
        ViewExtensions.onMemberChanged(_viewPager, ViewExtensions.SelectedIndexName, null);
        ViewExtensions.onMemberChanged(_viewPager, ViewExtensions.SelectedIndexEventName, null);
    }

    @Override
    public void addListener(Object target, String memberName) {
        if (ViewExtensions.SelectedIndexName.equals(memberName) || ViewExtensions.SelectedIndexEventName.equals(memberName) && _selectedIndexChangedCount++ == 0)
            _viewPager.registerOnPageChangeCallback(this);
    }

    @Override
    public void removeListener(Object target, String memberName) {
        if (ViewExtensions.SelectedIndexName.equals(memberName) || ViewExtensions.SelectedIndexEventName.equals(memberName) && _selectedIndexChangedCount != 0 && --_selectedIndexChangedCount == 0)
            _viewPager.unregisterOnPageChangeCallback(this);
    }
}
