package com.mugen.mvvm.internal;

import android.view.MenuItem;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.mugen.mvvm.constants.BindableMemberConstant;
import com.mugen.mvvm.constants.LifecycleState;
import com.mugen.mvvm.constants.PriorityConstant;
import com.mugen.mvvm.interfaces.ILifecycleDispatcher;
import com.mugen.mvvm.interfaces.views.IActivityView;
import com.mugen.mvvm.views.ActivityMugenExtensions;
import com.mugen.mvvm.views.BindableMemberMugenExtensions;

public final class ActionBarHomeClickListener implements ILifecycleDispatcher {
    @Override
    public boolean onLifecycleChanging(@NonNull Object target, int lifecycle, @Nullable Object state) {
        if (lifecycle == LifecycleState.OptionsItemSelected && state instanceof MenuItem && ((MenuItem) state).getItemId() == android.R.id.home) {
            Object actionBar = ActivityMugenExtensions.getActionBar((IActivityView) target);
            if (actionBar != null)
                BindableMemberMugenExtensions.onMemberChanged(actionBar, BindableMemberConstant.HomeButtonClick, null);
        }
        return true;
    }

    @Override
    public void onLifecycleChanged(@NonNull Object target, int lifecycle, @Nullable Object state) {
    }

    @Override
    public int getPriority() {
        return PriorityConstant.Default;
    }
}
