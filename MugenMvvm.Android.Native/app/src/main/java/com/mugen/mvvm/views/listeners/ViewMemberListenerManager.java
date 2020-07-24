package com.mugen.mvvm.views.listeners;

import android.view.View;
import android.widget.TextView;
import androidx.swiperefreshlayout.widget.SwipeRefreshLayout;
import androidx.viewpager.widget.ViewPager;
import androidx.viewpager2.widget.ViewPager2;
import com.mugen.mvvm.views.TextViewExtensions;
import com.mugen.mvvm.views.ViewExtensions;
import com.mugen.mvvm.views.ViewGroupExtensions;
import com.mugen.mvvm.views.support.SwipeRefreshLayoutExtensions;
import com.mugen.mvvm.views.support.ViewPager2Extensions;
import com.mugen.mvvm.views.support.ViewPagerExtensions;

import java.util.HashMap;
import java.util.Map;

public class ViewMemberListenerManager implements ViewExtensions.IMemberListenerManager {
    @Override
    public ViewExtensions.IMemberListener tryGetListener(HashMap<String, ViewExtensions.IMemberListener> listeners, View view, String memberName) {
        if (ViewExtensions.ClickEventName.equals(memberName) || isTextViewMember(view, memberName)) {
            if (listeners != null) {
                for (Map.Entry<String, ViewExtensions.IMemberListener> entry : listeners.entrySet()) {
                    if (entry.getValue() instanceof ViewMemberListener)
                        return entry.getValue();
                }
            }

            return new ViewMemberListener(view);
        }

        if (SwipeRefreshLayoutExtensions.RefreshedEventName.equals(memberName) && SwipeRefreshLayoutExtensions.isSupported(view))
            return new SwipeRefreshLayoutRefreshedListener((SwipeRefreshLayout) view);

        if (ViewGroupExtensions.SelectedIndexEventName.equals(memberName) || ViewGroupExtensions.SelectedIndexName.equals(memberName)) {
            if (ViewPagerExtensions.isSupported(view))
                return new ViewPagerSelectedIndexListener((ViewPager) view);
            if (ViewPager2Extensions.isSupported(view))
                return new ViewPager2SelectedIndexListener((ViewPager2) view);
        }

        return null;
    }

    private static boolean isTextViewMember(View view, String memberName) {
        return view instanceof TextView && (TextViewExtensions.TextEventName.equals(memberName) || TextViewExtensions.TextMemberName.equals(memberName));
    }
}
