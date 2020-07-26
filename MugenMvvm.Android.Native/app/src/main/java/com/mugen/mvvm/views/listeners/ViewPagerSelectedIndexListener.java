package com.mugen.mvvm.views.listeners;

import androidx.viewpager.widget.ViewPager;
import com.mugen.mvvm.views.ViewExtensions;
import com.mugen.mvvm.views.ViewGroupExtensions;

public class ViewPagerSelectedIndexListener implements ViewExtensions.IMemberListener, ViewPager.OnPageChangeListener {
    private final ViewPager _viewPager;
    private short _selectedIndexChangedCount;

    public ViewPagerSelectedIndexListener(ViewPager viewPager) {
        _viewPager = viewPager;
    }

    @Override
    public void onPageScrolled(int position, float positionOffset, int positionOffsetPixels) {

    }

    @Override
    public void onPageSelected(int position) {
        ViewExtensions.onMemberChanged(_viewPager, ViewGroupExtensions.SelectedIndexName, null);
        ViewExtensions.onMemberChanged(_viewPager, ViewGroupExtensions.SelectedIndexEventName, null);
    }

    @Override
    public void onPageScrollStateChanged(int state) {

    }

    @Override
    public void addListener(Object target, String memberName) {
        if (ViewGroupExtensions.SelectedIndexName.equals(memberName) || ViewGroupExtensions.SelectedIndexEventName.equals(memberName) && _selectedIndexChangedCount++ == 0)
            _viewPager.addOnPageChangeListener(this);
    }

    @Override
    public void removeListener(Object target, String memberName) {
        if (ViewGroupExtensions.SelectedIndexName.equals(memberName) || ViewGroupExtensions.SelectedIndexEventName.equals(memberName) && _selectedIndexChangedCount != 0 && --_selectedIndexChangedCount == 0)
            _viewPager.removeOnPageChangeListener(this);
    }
}
