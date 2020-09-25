package com.mugen.mvvm.views.listeners;

import com.google.android.material.tabs.TabLayout;
import com.mugen.mvvm.views.ViewExtensions;

public class TabLayoutSelectedIndexListener implements TabLayout.OnTabSelectedListener, ViewExtensions.IMemberListener {
    private final TabLayout _tabLayout;
    private short _selectedIndexChangedCount;

    public TabLayoutSelectedIndexListener(Object tabLayout) {
        _tabLayout = (TabLayout) tabLayout;
    }

    @Override
    public void onTabSelected(TabLayout.Tab tab) {
        ViewExtensions.onMemberChanged(_tabLayout, ViewExtensions.SelectedIndexName, tab);
        ViewExtensions.onMemberChanged(_tabLayout, ViewExtensions.SelectedIndexEventName, tab);
    }

    @Override
    public void onTabUnselected(TabLayout.Tab tab) {

    }

    @Override
    public void onTabReselected(TabLayout.Tab tab) {

    }

    @Override
    public void addListener(Object target, String memberName) {
        if (ViewExtensions.SelectedIndexName.equals(memberName) || ViewExtensions.SelectedIndexEventName.equals(memberName) && _selectedIndexChangedCount++ == 0)
            _tabLayout.addOnTabSelectedListener(this);
    }

    @Override
    public void removeListener(Object target, String memberName) {
        if (ViewExtensions.SelectedIndexName.equals(memberName) || ViewExtensions.SelectedIndexEventName.equals(memberName) && _selectedIndexChangedCount != 0 && --_selectedIndexChangedCount == 0)
            _tabLayout.removeOnTabSelectedListener(this);
    }
}
