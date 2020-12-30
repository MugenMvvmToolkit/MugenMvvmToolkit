package com.mugen.mvvm.views.listeners;

import android.view.View;
import android.widget.TextView;

import com.mugen.mvvm.constants.PriorityConstants;
import com.mugen.mvvm.views.ViewMugenExtensions;
import com.mugen.mvvm.views.support.SwipeRefreshLayoutMugenExtensions;
import com.mugen.mvvm.views.support.TabLayoutMugenExtensions;
import com.mugen.mvvm.views.support.ViewPager2MugenExtensions;
import com.mugen.mvvm.views.support.ViewPagerMugenExtensions;

import java.util.HashMap;
import java.util.Map;

public class ViewMemberListenerManager implements ViewMugenExtensions.IMemberListenerManager {
    private static boolean isTextViewMember(View view, String memberName) {
        return view instanceof TextView && (ViewMugenExtensions.TextEventName.equals(memberName) || ViewMugenExtensions.TextMemberName.equals(memberName));
    }

    @Override
    public ViewMugenExtensions.IMemberListener tryGetListener(HashMap<String, ViewMugenExtensions.IMemberListener> listeners, Object target, String memberName) {
        if (target instanceof View) {
            View view = (View) target;
            if (ViewMugenExtensions.ClickEventName.equals(memberName) || isTextViewMember(view, memberName)) {
                if (listeners != null) {
                    for (Map.Entry<String, ViewMugenExtensions.IMemberListener> entry : listeners.entrySet()) {
                        if (entry.getValue() instanceof ViewMemberListener)
                            return entry.getValue();
                    }
                }

                return new ViewMemberListener(view);
            }

            if (ViewMugenExtensions.RefreshedEventName.equals(memberName) && SwipeRefreshLayoutMugenExtensions.isSupported(view))
                return ViewMemberListenerUtils.getSwipeRefreshLayoutRefreshedListener(view);

            if (ViewMugenExtensions.SelectedIndexEventName.equals(memberName) || ViewMugenExtensions.SelectedIndexName.equals(memberName)) {
                if (ViewPagerMugenExtensions.isSupported(view))
                    return ViewMemberListenerUtils.getViewPagerSelectedIndexListener(target);
                if (ViewPager2MugenExtensions.isSupported(view))
                    return ViewMemberListenerUtils.getViewPager2SelectedIndexListener(target);
                if (TabLayoutMugenExtensions.isSupported(view))
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
