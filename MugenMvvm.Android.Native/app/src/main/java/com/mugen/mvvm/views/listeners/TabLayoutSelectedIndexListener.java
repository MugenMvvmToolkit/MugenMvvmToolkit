package com.mugen.mvvm.views.listeners;

import com.google.android.material.tabs.TabLayout;
import com.mugen.mvvm.views.ViewMugenExtensions;

public class TabLayoutSelectedIndexListener implements TabLayout.OnTabSelectedListener, ViewMugenExtensions.IMemberListener {
    private final TabLayout _tabLayout;
    private short _selectedIndexChangedCount;

    public TabLayoutSelectedIndexListener(Object tabLayout) {
        _tabLayout = (TabLayout) tabLayout;
    }

    @Override
    public void onTabSelected(TabLayout.Tab tab) {
        ViewMugenExtensions.onMemberChanged(_tabLayout, ViewMugenExtensions.SelectedIndexName, tab);
        ViewMugenExtensions.onMemberChanged(_tabLayout, ViewMugenExtensions.SelectedIndexEventName, tab);
    }

    @Override
    public void onTabUnselected(TabLayout.Tab tab) {

    }

    @Override
    public void onTabReselected(TabLayout.Tab tab) {

    }

    @Override
    public void addListener(Object target, String memberName) {
        if (ViewMugenExtensions.SelectedIndexName.equals(memberName) || ViewMugenExtensions.SelectedIndexEventName.equals(memberName) && _selectedIndexChangedCount++ == 0)
            _tabLayout.addOnTabSelectedListener(this);
    }

    @Override
    public void removeListener(Object target, String memberName) {
        if (ViewMugenExtensions.SelectedIndexName.equals(memberName) || ViewMugenExtensions.SelectedIndexEventName.equals(memberName) && _selectedIndexChangedCount != 0 && --_selectedIndexChangedCount == 0)
            _tabLayout.removeOnTabSelectedListener(this);
    }
}
