package com.mugen.mvvm.views.listeners;

import android.text.Editable;
import android.text.TextWatcher;
import android.view.KeyEvent;
import android.view.View;
import android.view.inputmethod.EditorInfo;
import android.widget.TextView;

import androidx.annotation.NonNull;

import com.mugen.mvvm.constants.BindableMemberConstant;
import com.mugen.mvvm.interfaces.IMemberListener;
import com.mugen.mvvm.views.BindableMemberMugenExtensions;

public class ViewMemberListener implements IMemberListener, View.OnClickListener, TextWatcher, View.OnLongClickListener, View.OnFocusChangeListener, TextView.OnEditorActionListener {
    protected final View View;
    private short _clickListenerCount;
    private short _longClickListenerCount;
    private short _textChangedListenerCount;
    private short _focusChangedListenerCount;
    private short _editorActionCount;

    public ViewMemberListener(View view) {
        View = view;
    }

    @Override
    public void addListener(@NonNull Object target, @NonNull String memberName) {
        if (BindableMemberConstant.Click.equals(memberName) && _clickListenerCount++ == 0)
            View.setOnClickListener(this);
        else if (BindableMemberConstant.LongClick.equals(memberName) && _longClickListenerCount++ == 0)
            View.setOnLongClickListener(this);
        else if (BindableMemberConstant.Text.equals(memberName) || BindableMemberConstant.TextEvent.equals(memberName) && _textChangedListenerCount++ == 0)
            ((TextView) target).addTextChangedListener(this);
        else if (BindableMemberConstant.IsFocused.equals(memberName) || BindableMemberConstant.FocusChangedEvent.equals(memberName) && _focusChangedListenerCount++ == 0)
            View.setOnFocusChangeListener(this);
        else if (BindableMemberConstant.ImeActionEvent.equals(memberName) && _editorActionCount++ == 0)
            ((TextView) target).setOnEditorActionListener(this);
    }

    @Override
    public void removeListener(@NonNull Object target, @NonNull String memberName) {
        if (BindableMemberConstant.Click.equals(memberName) && _clickListenerCount != 0 && --_clickListenerCount == 0)
            View.setOnClickListener(null);
        else if (BindableMemberConstant.LongClick.equals(memberName) && _longClickListenerCount != 0 && --_longClickListenerCount == 0)
            View.setOnLongClickListener(null);
        else if (BindableMemberConstant.Text.equals(memberName) || BindableMemberConstant.TextEvent.equals(memberName) && _textChangedListenerCount != 0 && --_textChangedListenerCount == 0)
            ((TextView) target).removeTextChangedListener(this);
        else if (BindableMemberConstant.IsFocused.equals(memberName) || BindableMemberConstant.FocusChangedEvent.equals(memberName) && --_focusChangedListenerCount == 0)
            View.setOnFocusChangeListener(null);
        else if (BindableMemberConstant.ImeActionEvent.equals(memberName) && --_editorActionCount == 0)
            ((TextView) target).setOnEditorActionListener(this);
    }

    @Override
    public void beforeTextChanged(CharSequence s, int start, int count, int after) {

    }

    @Override
    public void onTextChanged(CharSequence s, int start, int before, int count) {
        BindableMemberMugenExtensions.onMemberChanged(View, BindableMemberConstant.Text, null);
        BindableMemberMugenExtensions.onMemberChanged(View, BindableMemberConstant.TextEvent, null);
    }

    @Override
    public void afterTextChanged(Editable s) {

    }

    @Override
    public void onClick(View v) {
        BindableMemberMugenExtensions.onMemberChanged(v, BindableMemberConstant.Click, null);
    }

    @Override
    public boolean onLongClick(android.view.View v) {
        return BindableMemberMugenExtensions.onMemberChanged(v, BindableMemberConstant.LongClick, null);
    }

    @Override
    public void onFocusChange(View view, boolean b) {
        BindableMemberMugenExtensions.onMemberChanged(view, BindableMemberConstant.IsFocused, null);
        BindableMemberMugenExtensions.onMemberChanged(view, BindableMemberConstant.FocusChangedEvent, null);
    }

    @Override
    public boolean onEditorAction(TextView textView, int actionId, KeyEvent keyEvent) {
        if (textView.getImeActionId() == actionId || actionId == EditorInfo.IME_NULL) {
            BindableMemberMugenExtensions.onMemberChanged(textView, BindableMemberConstant.ImeActionEvent, keyEvent);
            return true;
        }
        return false;
    }
}
