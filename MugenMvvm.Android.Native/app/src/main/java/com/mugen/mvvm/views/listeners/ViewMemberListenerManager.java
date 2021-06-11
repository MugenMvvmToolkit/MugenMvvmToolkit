package com.mugen.mvvm.views.listeners;

import android.view.View;
import android.widget.CompoundButton;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.mugen.mvvm.constants.BindableMemberConstant;
import com.mugen.mvvm.constants.PriorityConstant;
import com.mugen.mvvm.interfaces.IMemberListener;
import com.mugen.mvvm.interfaces.IMemberListenerManager;
import com.mugen.mvvm.views.support.SwipeRefreshLayoutMugenExtensions;
import com.mugen.mvvm.views.support.TabLayoutMugenExtensions;
import com.mugen.mvvm.views.support.ViewPager2MugenExtensions;
import com.mugen.mvvm.views.support.ViewPagerMugenExtensions;

import java.util.HashMap;
import java.util.Map;

public class ViewMemberListenerManager implements IMemberListenerManager {
    private static boolean isTextViewMember(View view, String memberName) {
        return view instanceof TextView && (BindableMemberConstant.TextEvent.equals(memberName) || BindableMemberConstant.Text.equals(memberName));
    }

    @Nullable
    @Override
    public IMemberListener tryGetListener(@NonNull Object target, @NonNull String memberName, @Nullable HashMap<String, IMemberListener> listeners) {
        if (target instanceof View) {
            View view = (View) target;
            if (BindableMemberConstant.Click.equals(memberName) || isTextViewMember(view, memberName)) {
                if (listeners != null) {
                    for (Map.Entry<String, IMemberListener> entry : listeners.entrySet()) {
                        if (entry.getValue() instanceof ViewMemberListener)
                            return entry.getValue();
                    }
                }

                return new ViewMemberListener(view);
            }

            if (target instanceof CompoundButton && (BindableMemberConstant.Checked.equals(memberName) || BindableMemberConstant.CheckedEvent.equals(memberName)))
                return new CheckedChangeListener();

            if (BindableMemberConstant.RefreshedEvent.equals(memberName) && SwipeRefreshLayoutMugenExtensions.isSupported(view))
                return ViewMemberListenerUtils.getSwipeRefreshLayoutRefreshedListener(view);

            if (BindableMemberConstant.SelectedIndexEvent.equals(memberName) || BindableMemberConstant.SelectedIndex.equals(memberName)) {
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
        return PriorityConstant.Default;
    }
}
