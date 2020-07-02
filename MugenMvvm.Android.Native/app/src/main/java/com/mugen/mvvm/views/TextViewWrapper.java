package com.mugen.mvvm.views;

import android.text.Editable;
import android.text.TextWatcher;
import android.view.View;
import android.widget.TextView;
import com.mugen.mvvm.interfaces.IMemberObserver;
import com.mugen.mvvm.interfaces.views.ITextView;

public class TextViewWrapper extends ViewWrapper implements TextWatcher, ITextView {
    public static final String TextMemberName = "Text";
    public static final String TextEventName = "TextChanged";

    private short _textChangedListenerCount;

    //todo using Object as type to prevent xamarin linker keep View
    public TextViewWrapper(Object view) {
        super((TextView) view);
    }

    @Override
    public CharSequence getText() {
        TextView view = (TextView) getView();
        if (view == null)
            return null;
        return view.getText();
    }

    @Override
    public void setText(CharSequence text) {
        TextView view = (TextView) getView();
        if (view != null)
            view.setText(text);
    }

    @Override
    public void setTextColor(int color) {
        TextView view = (TextView) getView();
        if (view != null)
            view.setTextColor(color);
    }

    @Override
    public void beforeTextChanged(CharSequence s, int start, int count, int after) {
    }

    @Override
    public void onTextChanged(CharSequence s, int start, int before, int count) {
        onMemberChanged(TextMemberName, null);
    }

    @Override
    public void afterTextChanged(Editable s) {
    }

    @Override
    protected void onMemberChanged(IMemberObserver observer, String member, Object args) {
        super.onMemberChanged(observer, member, args);
        if (TextMemberName.equals(member))
            onMemberChanged(TextEventName, null);
    }

    @Override
    protected void addMemberListener(View view, String memberName) {
        super.addMemberListener(view, memberName);
        if (TextMemberName.equals(memberName) || TextEventName.equals(memberName) && _textChangedListenerCount++ == 0)
            ((TextView) view).addTextChangedListener(this);
    }

    @Override
    protected void removeMemberListener(View view, String memberName) {
        super.removeMemberListener(view, memberName);
        if (TextMemberName.equals(memberName) || TextEventName.equals(memberName) && _textChangedListenerCount != 0 && --_textChangedListenerCount == 0)
            ((TextView) view).removeTextChangedListener(this);
    }
}
