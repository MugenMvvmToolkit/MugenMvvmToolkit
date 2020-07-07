package com.mugen.mvvm.interfaces.views;

public interface ITextView extends IAndroidView {
    String TextMemberName = "Text";
    String TextEventName = "TextChanged";

    CharSequence getText();

    void setText(CharSequence text);

    void setTextColor(int color);
}
