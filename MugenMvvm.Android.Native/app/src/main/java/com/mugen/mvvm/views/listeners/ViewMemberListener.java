package com.mugen.mvvm.views.listeners;

import android.text.Editable;
import android.text.TextWatcher;
import android.view.View;
import android.widget.TextView;
import com.mugen.mvvm.views.TextViewExtensions;
import com.mugen.mvvm.views.ViewExtensions;

public class ViewMemberListener implements ViewExtensions.IMemberListener, View.OnClickListener, TextWatcher {
    protected final View View;
    private short _clickListenerCount;
    private short _textChangedListenerCount;

    public ViewMemberListener(View view) {
        View = view;
    }

    @Override
    public void addListener(View view, String memberName) {
        if (ViewExtensions.ClickEventName.equals(memberName) && _clickListenerCount++ == 0)
            view.setOnClickListener(this);
        else if (TextViewExtensions.TextMemberName.equals(memberName) || TextViewExtensions.TextEventName.equals(memberName) && _textChangedListenerCount++ == 0)
            ((TextView) view).addTextChangedListener(this);
    }

    @Override
    public void removeListener(View view, String memberName) {
        if (ViewExtensions.ClickEventName.equals(memberName) && _clickListenerCount != 0 && --_clickListenerCount == 0)
            view.setOnClickListener(null);
        else if (TextViewExtensions.TextMemberName.equals(memberName) || TextViewExtensions.TextEventName.equals(memberName) && _textChangedListenerCount != 0 && --_textChangedListenerCount == 0)
            ((TextView) view).removeTextChangedListener(this);
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
}
