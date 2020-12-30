package com.mugen.mvvm.views;

import android.view.View;
import android.widget.TextView;

public final class TextViewMugenExtensions {
    private TextViewMugenExtensions() {
    }

    public static String getText(View view) {
        CharSequence txt = getTextRaw(view);
        if (txt == null)
            return null;
        return txt.toString();
    }

    public static void setText(View view, String text) {
        setTextRaw(view, text);
    }

    public static CharSequence getTextRaw(View view) {
        return ((TextView) view).getText();
    }

    public static void setTextRaw(View view, CharSequence text) {
        ((TextView) view).setText(text);
    }
}
