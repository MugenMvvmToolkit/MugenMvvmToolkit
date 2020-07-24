package com.mugen.mvvm.views;

import android.view.View;
import android.widget.TextView;

public abstract class TextViewExtensions extends ViewExtensions {
    public static String TextMemberName = "Text";
    public static String TextEventName = "TextChanged";

    public static CharSequence getText(View view) {
        return ((TextView) view).getText();
    }

    public static void setText(View view, CharSequence text) {
        ((TextView) view).setText(text);
    }
}
