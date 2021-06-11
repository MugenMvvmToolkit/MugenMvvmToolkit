package com.mugen.mvvm.views.listeners;

import androidx.annotation.NonNull;
import androidx.viewpager2.widget.ViewPager2;

import com.mugen.mvvm.constants.BindableMemberConstant;
import com.mugen.mvvm.interfaces.IMemberListener;
import com.mugen.mvvm.views.BindableMemberMugenExtensions;

public class ViewPager2SelectedIndexListener extends ViewPager2.OnPageChangeCallback implements IMemberListener {
    private final ViewPager2 _viewPager;
    private short _selectedIndexChangedCount;

    public ViewPager2SelectedIndexListener(Object viewPager) {
        _viewPager = (ViewPager2) viewPager;
    }

    @Override
    public void onPageSelected(int position) {
        BindableMemberMugenExtensions.onMemberChanged(_viewPager, BindableMemberConstant.SelectedIndex, null);
        BindableMemberMugenExtensions.onMemberChanged(_viewPager, BindableMemberConstant.SelectedIndexEvent, null);
    }

    @Override
    public void addListener(@NonNull Object target, @NonNull String memberName) {
        if (BindableMemberConstant.SelectedIndex.equals(memberName) || BindableMemberConstant.SelectedIndexEvent.equals(memberName) && _selectedIndexChangedCount++ == 0)
            _viewPager.registerOnPageChangeCallback(this);
    }

    @Override
    public void removeListener(@NonNull Object target, @NonNull String memberName) {
        if (BindableMemberConstant.SelectedIndex.equals(memberName) || BindableMemberConstant.SelectedIndexEvent.equals(memberName) && _selectedIndexChangedCount != 0 && --_selectedIndexChangedCount == 0)
            _viewPager.unregisterOnPageChangeCallback(this);
    }
}
