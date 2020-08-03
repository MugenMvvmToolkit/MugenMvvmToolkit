package com.mugen.mvvm.views.listeners;

import android.view.View;
import android.widget.TextView;
import androidx.swiperefreshlayout.widget.SwipeRefreshLayout;
import androidx.viewpager.widget.ViewPager;
import androidx.viewpager2.widget.ViewPager2;
import com.google.android.material.tabs.TabLayout;
import com.mugen.mvvm.constants.PriorityConstants;
import com.mugen.mvvm.views.ViewExtensions;
import com.mugen.mvvm.views.support.SwipeRefreshLayoutExtensions;
import com.mugen.mvvm.views.support.TabLayoutExtensions;
import com.mugen.mvvm.views.support.ViewPager2Extensions;
import com.mugen.mvvm.views.support.ViewPagerExtensions;

import java.util.HashMap;
import java.util.Map;

public class ViewMemberListenerManager implements ViewExtensions.IMemberListenerManager {
    @Override
    public ViewExtensions.IMemberListener tryGetListener(HashMap<String, ViewExtensions.IMemberListener> listeners, Object target, String memberName) {
        if (target instanceof View) {
            View view = (View) target;
            if (ViewExtensions.ClickEventName.equals(memberName) || isTextViewMember(view, memberName)) {
                if (listeners != null) {
                    for (Map.Entry<String, ViewExtensions.IMemberListener> entry : listeners.entrySet()) {
                        if (entry.getValue() instanceof ViewMemberListener)
                            return entry.getValue();
                    }
                }

                return new ViewMemberListener(view);
            }

            if (ViewExtensions.RefreshedEventName.equals(memberName) && SwipeRefreshLayoutExtensions.isSupported(view))
                return new SwipeRefreshLayoutRefreshedListener((SwipeRefreshLayout) target);

            if (ViewExtensions.SelectedIndexEventName.equals(memberName) || ViewExtensions.SelectedIndexName.equals(memberName)) {
                if (ViewPagerExtensions.isSupported(view))
                    return new ViewPagerSelectedIndexListener((ViewPager) target);
                if (ViewPager2Extensions.isSupported(view))
                    return new ViewPager2SelectedIndexListener((ViewPager2) target);
                if (TabLayoutExtensions.isSupported(view))
                    return new TabLayoutSelectedIndexListener((TabLayout) target);
            }
        }


        return null;
    }

    @Override
    public int getPriority() {
        return PriorityConstants.Default;
    }

    private static boolean isTextViewMember(View view, String memberName) {
        return view instanceof TextView && (ViewExtensions.TextEventName.equals(memberName) || ViewExtensions.TextMemberName.equals(memberName));
    }
}
