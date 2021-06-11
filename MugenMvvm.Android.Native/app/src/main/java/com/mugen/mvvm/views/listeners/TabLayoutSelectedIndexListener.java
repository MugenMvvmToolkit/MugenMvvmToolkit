package com.mugen.mvvm.views.listeners;

import androidx.annotation.NonNull;

import com.google.android.material.tabs.TabLayout;
import com.mugen.mvvm.constants.BindableMemberConstant;
import com.mugen.mvvm.interfaces.IMemberListener;
import com.mugen.mvvm.views.BindableMemberMugenExtensions;

public class TabLayoutSelectedIndexListener implements TabLayout.OnTabSelectedListener, IMemberListener {
    private final TabLayout _tabLayout;
    private short _selectedIndexChangedCount;

    public TabLayoutSelectedIndexListener(Object tabLayout) {
        _tabLayout = (TabLayout) tabLayout;
    }

    @Override
    public void onTabSelected(TabLayout.Tab tab) {
        BindableMemberMugenExtensions.onMemberChanged(_tabLayout, BindableMemberConstant.SelectedIndex, tab);
        BindableMemberMugenExtensions.onMemberChanged(_tabLayout, BindableMemberConstant.SelectedIndexEvent, tab);
    }

    @Override
    public void onTabUnselected(TabLayout.Tab tab) {

    }

    @Override
    public void onTabReselected(TabLayout.Tab tab) {

    }

    @Override
    public void addListener(@NonNull Object target, @NonNull String memberName) {
        if (BindableMemberConstant.SelectedIndex.equals(memberName) || BindableMemberConstant.SelectedIndexEvent.equals(memberName) && _selectedIndexChangedCount++ == 0)
            _tabLayout.addOnTabSelectedListener(this);
    }

    @Override
    public void removeListener(@NonNull Object target, @NonNull String memberName) {
        if (BindableMemberConstant.SelectedIndex.equals(memberName) || BindableMemberConstant.SelectedIndexEvent.equals(memberName) && _selectedIndexChangedCount != 0 && --_selectedIndexChangedCount == 0)
            _tabLayout.removeOnTabSelectedListener(this);
    }
}
