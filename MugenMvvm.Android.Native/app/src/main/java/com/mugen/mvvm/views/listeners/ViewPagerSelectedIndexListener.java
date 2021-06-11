package com.mugen.mvvm.views.listeners;

import androidx.annotation.NonNull;
import androidx.viewpager.widget.ViewPager;

import com.mugen.mvvm.constants.BindableMemberConstant;
import com.mugen.mvvm.interfaces.IMemberListener;
import com.mugen.mvvm.views.BindableMemberMugenExtensions;

public class ViewPagerSelectedIndexListener implements IMemberListener, ViewPager.OnPageChangeListener {
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
        BindableMemberMugenExtensions.onMemberChanged(_viewPager, BindableMemberConstant.SelectedIndex, null);
        BindableMemberMugenExtensions.onMemberChanged(_viewPager, BindableMemberConstant.SelectedIndexEvent, null);
    }

    @Override
    public void onPageScrollStateChanged(int state) {

    }

    @Override
    public void addListener(@NonNull Object target, @NonNull String memberName) {
        if (BindableMemberConstant.SelectedIndex.equals(memberName) || BindableMemberConstant.SelectedIndexEvent.equals(memberName) && _selectedIndexChangedCount++ == 0)
            _viewPager.addOnPageChangeListener(this);
    }

    @Override
    public void removeListener(@NonNull Object target, @NonNull String memberName) {
        if (BindableMemberConstant.SelectedIndex.equals(memberName) || BindableMemberConstant.SelectedIndexEvent.equals(memberName) && _selectedIndexChangedCount != 0 && --_selectedIndexChangedCount == 0)
            _viewPager.removeOnPageChangeListener(this);
    }
}
