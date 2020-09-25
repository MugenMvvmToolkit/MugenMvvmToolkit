package com.mugen.mvvm.views.listeners;

import androidx.viewpager.widget.ViewPager;

import com.mugen.mvvm.views.ViewExtensions;

public class ViewPagerSelectedIndexListener implements ViewExtensions.IMemberListener, ViewPager.OnPageChangeListener {
    private final ViewPager _viewPager;
    private short _selectedIndexChangedCount;

    public ViewPagerSelectedIndexListener(Object viewPager) {
        _viewPager = (ViewPager) viewPager;
    }

    @Override
    public void onPageScrolled(int position, float positionOffset, int positionOffsetPixels) {

    }

    @Override
    public void onPageSelected(int position) {
        ViewExtensions.onMemberChanged(_viewPager, ViewExtensions.SelectedIndexName, null);
        ViewExtensions.onMemberChanged(_viewPager, ViewExtensions.SelectedIndexEventName, null);
    }

    @Override
    public void onPageScrollStateChanged(int state) {

    }

    @Override
    public void addListener(Object target, String memberName) {
        if (ViewExtensions.SelectedIndexName.equals(memberName) || ViewExtensions.SelectedIndexEventName.equals(memberName) && _selectedIndexChangedCount++ == 0)
            _viewPager.addOnPageChangeListener(this);
    }

    @Override
    public void removeListener(Object target, String memberName) {
        if (ViewExtensions.SelectedIndexName.equals(memberName) || ViewExtensions.SelectedIndexEventName.equals(memberName) && _selectedIndexChangedCount != 0 && --_selectedIndexChangedCount == 0)
            _viewPager.removeOnPageChangeListener(this);
    }
}
