package com.mugen.mvvm.views.listeners;

import android.view.View;
import androidx.viewpager2.widget.ViewPager2;
import com.mugen.mvvm.views.ViewExtensions;
import com.mugen.mvvm.views.ViewGroupExtensions;

public class ViewPager2SelectedIndexListener extends ViewPager2.OnPageChangeCallback implements ViewExtensions.IMemberListener {
    private final ViewPager2 _viewPager;
    private short _selectedIndexChangedCount;

    public ViewPager2SelectedIndexListener(ViewPager2 viewPager) {
        _viewPager = viewPager;
    }

    @Override
    public void onPageSelected(int position) {
        ViewExtensions.onMemberChanged(_viewPager, ViewGroupExtensions.SelectedIndexName, null);
        ViewExtensions.onMemberChanged(_viewPager, ViewGroupExtensions.SelectedIndexEventName, null);
    }

    @Override
    public void addListener(View view, String memberName) {
        if (ViewGroupExtensions.SelectedIndexName.equals(memberName) || ViewGroupExtensions.SelectedIndexEventName.equals(memberName) && _selectedIndexChangedCount++ == 0)
            _viewPager.registerOnPageChangeCallback(this);
    }

    @Override
    public void removeListener(View view, String memberName) {
        if (ViewGroupExtensions.SelectedIndexName.equals(memberName) || ViewGroupExtensions.SelectedIndexEventName.equals(memberName) && _selectedIndexChangedCount != 0 && --_selectedIndexChangedCount == 0)
            _viewPager.unregisterOnPageChangeCallback(this);
    }
}
