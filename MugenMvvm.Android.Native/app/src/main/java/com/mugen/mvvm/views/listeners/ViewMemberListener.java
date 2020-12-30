package com.mugen.mvvm.views.listeners;

import android.text.Editable;
import android.text.TextWatcher;
import android.view.View;
import android.widget.TextView;

import com.mugen.mvvm.views.ViewMugenExtensions;

public class ViewMemberListener implements ViewMugenExtensions.IMemberListener, View.OnClickListener, TextWatcher, android.view.View.OnLongClickListener {
    protected final View View;
    private short _clickListenerCount;
    private short _longClickListenerCount;
    private short _textChangedListenerCount;

    public ViewMemberListener(View view) {
        View = view;
    }

    @Override
    public void addListener(Object target, String memberName) {
        if (ViewMugenExtensions.ClickEventName.equals(memberName) && _clickListenerCount++ == 0)
            View.setOnClickListener(this);
        else if (ViewMugenExtensions.LongClickEventName.equals(memberName) && _longClickListenerCount++ == 0)
            View.setOnLongClickListener(this);
        else if (ViewMugenExtensions.TextMemberName.equals(memberName) || ViewMugenExtensions.TextEventName.equals(memberName) && _textChangedListenerCount++ == 0)
            ((TextView) target).addTextChangedListener(this);
    }

    @Override
    public void removeListener(Object target, String memberName) {
        if (ViewMugenExtensions.ClickEventName.equals(memberName) && _clickListenerCount != 0 && --_clickListenerCount == 0)
            View.setOnClickListener(null);
        else if (ViewMugenExtensions.LongClickEventName.equals(memberName) && _longClickListenerCount != 0 && --_longClickListenerCount == 0)
            View.setOnLongClickListener(null);
        else if (ViewMugenExtensions.TextMemberName.equals(memberName) || ViewMugenExtensions.TextEventName.equals(memberName) && _textChangedListenerCount != 0 && --_textChangedListenerCount == 0)
            ((TextView) target).removeTextChangedListener(this);
    }

    @Override
    public void beforeTextChanged(CharSequence s, int start, int count, int after) {

    }

    @Override
    public void onTextChanged(CharSequence s, int start, int before, int count) {
        ViewMugenExtensions.onMemberChanged(View, ViewMugenExtensions.TextMemberName, null);
        ViewMugenExtensions.onMemberChanged(View, ViewMugenExtensions.TextEventName, null);
    }

    @Override
    public void afterTextChanged(Editable s) {

    }

    @Override
    public void onClick(View v) {
        ViewMugenExtensions.onMemberChanged(v, ViewMugenExtensions.ClickEventName, null);
    }

    @Override
    public boolean onLongClick(android.view.View v) {
        return ViewMugenExtensions.onMemberChanged(v, ViewMugenExtensions.LongClickEventName, null);
    }
}
