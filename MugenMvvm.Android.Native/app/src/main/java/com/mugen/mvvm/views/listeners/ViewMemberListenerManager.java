package com.mugen.mvvm.views.listeners;

import android.view.View;
import android.widget.CompoundButton;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.mugen.mvvm.constants.PriorityConstants;
import com.mugen.mvvm.interfaces.IMemberListener;
import com.mugen.mvvm.interfaces.IMemberListenerManager;
import com.mugen.mvvm.views.BindableMemberMugenExtensions;
import com.mugen.mvvm.views.support.SwipeRefreshLayoutMugenExtensions;
import com.mugen.mvvm.views.support.TabLayoutMugenExtensions;
import com.mugen.mvvm.views.support.ViewPager2MugenExtensions;
import com.mugen.mvvm.views.support.ViewPagerMugenExtensions;

import java.util.HashMap;
import java.util.Map;

public class ViewMemberListenerManager implements IMemberListenerManager {
    private static boolean isTextViewMember(View view, String memberName) {
        return view instanceof TextView && (BindableMemberMugenExtensions.TextEventName.equals(memberName) || BindableMemberMugenExtensions.TextMemberName.equals(memberName));
    }

    @Nullable
    @Override
    public IMemberListener tryGetListener(@NonNull Object target, @NonNull String memberName, @Nullable HashMap<String, IMemberListener> listeners) {
        if (target instanceof View) {
            View view = (View) target;
            if (BindableMemberMugenExtensions.ClickEventName.equals(memberName) || isTextViewMember(view, memberName)) {
                if (listeners != null) {
                    for (Map.Entry<String, IMemberListener> entry : listeners.entrySet()) {
                        if (entry.getValue() instanceof ViewMemberListener)
                            return entry.getValue();
                    }
                }

                return new ViewMemberListener(view);
            }

            if (target instanceof CompoundButton && (BindableMemberMugenExtensions.CheckedMemberName.equals(memberName) || BindableMemberMugenExtensions.CheckedEventName.equals(memberName)))
                return new CheckedChangeListener();

            if (BindableMemberMugenExtensions.RefreshedEventName.equals(memberName) && SwipeRefreshLayoutMugenExtensions.isSupported(view))
                return ViewMemberListenerUtils.getSwipeRefreshLayoutRefreshedListener(view);

            if (BindableMemberMugenExtensions.SelectedIndexEventName.equals(memberName) || BindableMemberMugenExtensions.SelectedIndexName.equals(memberName)) {
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
