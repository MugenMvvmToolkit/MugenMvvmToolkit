package com.mugen.mvvm.views.listeners;

import android.widget.CompoundButton;

import androidx.annotation.NonNull;

import com.mugen.mvvm.interfaces.IMemberListener;
import com.mugen.mvvm.views.BindableMemberMugenExtensions;

public class CheckedChangeListener implements IMemberListener, CompoundButton.OnCheckedChangeListener {
    private short _listenerCount;

    @Override
    public void addListener(@NonNull Object target, @NonNull String memberName) {
        if ((BindableMemberMugenExtensions.CheckedMemberName.equals(memberName) || BindableMemberMugenExtensions.CheckedEventName.equals(memberName)) && _listenerCount++ == 0)
            ((CompoundButton) target).setOnCheckedChangeListener(this);
    }

    @Override
    public void removeListener(@NonNull Object target, @NonNull String memberName) {
        if ((BindableMemberMugenExtensions.CheckedMemberName.equals(memberName) || BindableMemberMugenExtensions.CheckedEventName.equals(memberName)) && --_listenerCount == 0)
            ((CompoundButton) target).setOnCheckedChangeListener(null);
    }

    @Override
    public void onCheckedChanged(CompoundButton buttonView, boolean isChecked) {
        BindableMemberMugenExtensions.onMemberChanged(buttonView, BindableMemberMugenExtensions.CheckedMemberName, null);
        BindableMemberMugenExtensions.onMemberChanged(buttonView, BindableMemberMugenExtensions.CheckedEventName, null);
    }
}
