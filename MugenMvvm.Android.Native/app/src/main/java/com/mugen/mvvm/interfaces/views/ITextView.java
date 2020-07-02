package com.mugen.mvvm.interfaces.views;

public interface ITextView extends IAndroidView {
    CharSequence getText();

    void setText(CharSequence text);

    void setTextColor(int color);
}
