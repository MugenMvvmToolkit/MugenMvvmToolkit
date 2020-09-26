package com.mugen.mvvm.views.listeners;

import android.view.View;
import android.widget.TextView;

import com.mugen.mvvm.constants.PriorityConstants;
import com.mugen.mvvm.views.ViewExtensions;
import com.mugen.mvvm.views.support.SwipeRefreshLayoutExtensions;
import com.mugen.mvvm.views.support.TabLayoutExtensions;
import com.mugen.mvvm.views.support.ViewPager2Extensions;
import com.mugen.mvvm.views.support.ViewPagerExtensions;

import java.util.HashMap;
import java.util.Map;

public class ViewMemberListenerManager implements ViewExtensions.IMemberListenerManager {
    private static boolean isTextViewMember(View view, String memberName) {
        return view instanceof TextView && (ViewExtensions.TextEventName.equals(memberName) || ViewExtensions.TextMemberName.equals(memberName));
    }

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
                return ViewMemberListenerUtils.getSwipeRefreshLayoutRefreshedListener(view);

            if (ViewExtensions.SelectedIndexEventName.equals(memberName) || ViewExtensions.SelectedIndexName.equals(memberName)) {
                if (ViewPagerExtensions.isSupported(view))
                    return ViewMemberListenerUtils.getViewPagerSelectedIndexListener(target);
                if (ViewPager2Extensions.isSupported(view))
                    return ViewMemberListenerUtils.getViewPager2SelectedIndexListener(target);
                if (TabLayoutExtensions.isSupported(view))
                    return ViewMemberListenerUtils.getTabLayoutSelectedIndexListener(target);
            }
        }

        return null;
    }

    @Override
    public int getPriority() {
        return PriorityConstants.Default;
    }
}
