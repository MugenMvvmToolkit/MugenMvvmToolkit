package com.mugen.mvvm.views.listeners;

import android.text.Editable;
import android.text.TextWatcher;
import android.view.View;
import android.widget.TextView;
import com.mugen.mvvm.views.TextViewExtensions;
import com.mugen.mvvm.views.ViewExtensions;

public class ViewMemberListener implements ViewExtensions.IMemberListener, View.OnClickListener, TextWatcher, android.view.View.OnLongClickListener {
    protected final View View;
    private short _clickListenerCount;
    private short _longClickListenerCount;
    private short _textChangedListenerCount;

    public ViewMemberListener(View view) {
        View = view;
    }

    @Override
    public void addListener(Object target, String memberName) {
        if (ViewExtensions.ClickEventName.equals(memberName) && _clickListenerCount++ == 0)
            View.setOnClickListener(this);
        else if (ViewExtensions.LongClickEventName.equals(memberName) && _longClickListenerCount++ == 0)
            View.setOnLongClickListener(this);
        else if (TextViewExtensions.TextMemberName.equals(memberName) || TextViewExtensions.TextEventName.equals(memberName) && _textChangedListenerCount++ == 0)
            ((TextView) target).addTextChangedListener(this);
    }

    @Override
    public void removeListener(Object target, String memberName) {
        if (ViewExtensions.ClickEventName.equals(memberName) && _clickListenerCount != 0 && --_clickListenerCount == 0)
            View.setOnClickListener(null);
        else if (ViewExtensions.LongClickEventName.equals(memberName) && _longClickListenerCount != 0 && --_longClickListenerCount == 0)
            View.setOnLongClickListener(null);
        else if (TextViewExtensions.TextMemberName.equals(memberName) || TextViewExtensions.TextEventName.equals(memberName) && _textChangedListenerCount != 0 && --_textChangedListenerCount == 0)
            ((TextView) target).removeTextChangedListener(this);
    }

    @Override
    public void beforeTextChanged(CharSequence s, int start, int count, int after) {

    }

    @Override
    public void onTextChanged(CharSequence s, int start, int before, int count) {
        ViewExtensions.onMemberChanged(View, TextViewExtensions.TextMemberName, null);
        ViewExtensions.onMemberChanged(View, TextViewExtensions.TextEventName, null);
    }

    @Override
    public void afterTextChanged(Editable s) {

    }

    @Override
    public void onClick(View v) {
        ViewExtensions.onMemberChanged(v, ViewExtensions.ClickEventName, null);
    }

    @Override
    public boolean onLongClick(android.view.View v) {
        return ViewExtensions.onMemberChanged(v, ViewExtensions.LongClickEventName, null);
    }
}
