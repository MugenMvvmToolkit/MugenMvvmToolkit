package com.mugen.mvvm.views.listeners;

import android.text.Editable;
import android.text.TextWatcher;
import android.view.View;
import android.widget.TextView;

import androidx.annotation.NonNull;

import com.mugen.mvvm.interfaces.IMemberListener;
import com.mugen.mvvm.views.BindableMemberMugenExtensions;
import com.mugen.mvvm.views.ViewMugenExtensions;

public class ViewMemberListener implements IMemberListener, View.OnClickListener, TextWatcher, android.view.View.OnLongClickListener {
    protected final View View;
    private short _clickListenerCount;
    private short _longClickListenerCount;
    private short _textChangedListenerCount;

    public ViewMemberListener(View view) {
        View = view;
    }

    @Override
    public void addListener(@NonNull Object target, @NonNull String memberName) {
        if (BindableMemberMugenExtensions.ClickEventName.equals(memberName) && _clickListenerCount++ == 0)
            View.setOnClickListener(this);
        else if (BindableMemberMugenExtensions.LongClickEventName.equals(memberName) && _longClickListenerCount++ == 0)
            View.setOnLongClickListener(this);
        else if (BindableMemberMugenExtensions.TextMemberName.equals(memberName) || BindableMemberMugenExtensions.TextEventName.equals(memberName) && _textChangedListenerCount++ == 0)
            ((TextView) target).addTextChangedListener(this);
    }

    @Override
    public void removeListener(@NonNull Object target, @NonNull String memberName) {
        if (BindableMemberMugenExtensions.ClickEventName.equals(memberName) && _clickListenerCount != 0 && --_clickListenerCount == 0)
            View.setOnClickListener(null);
        else if (BindableMemberMugenExtensions.LongClickEventName.equals(memberName) && _longClickListenerCount != 0 && --_longClickListenerCount == 0)
            View.setOnLongClickListener(null);
        else if (BindableMemberMugenExtensions.TextMemberName.equals(memberName) || BindableMemberMugenExtensions.TextEventName.equals(memberName) && _textChangedListenerCount != 0 && --_textChangedListenerCount == 0)
            ((TextView) target).removeTextChangedListener(this);
    }

    @Override
    public void beforeTextChanged(CharSequence s, int start, int count, int after) {

    }

    @Override
    public void onTextChanged(CharSequence s, int start, int before, int count) {
        BindableMemberMugenExtensions.onMemberChanged(View, BindableMemberMugenExtensions.TextMemberName, null);
        BindableMemberMugenExtensions.onMemberChanged(View, BindableMemberMugenExtensions.TextEventName, null);
    }

    @Override
    public void afterTextChanged(Editable s) {

    }

    @Override
    public void onClick(View v) {
        BindableMemberMugenExtensions.onMemberChanged(v, BindableMemberMugenExtensions.ClickEventName, null);
    }

    @Override
    public boolean onLongClick(android.view.View v) {
        return BindableMemberMugenExtensions.onMemberChanged(v, BindableMemberMugenExtensions.LongClickEventName, null);
    }
}
