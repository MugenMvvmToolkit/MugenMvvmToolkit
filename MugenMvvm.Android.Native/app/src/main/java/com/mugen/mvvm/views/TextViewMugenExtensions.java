package com.mugen.mvvm.views;

import android.view.View;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

public final class TextViewMugenExtensions {
    private TextViewMugenExtensions() {
    }

    @Nullable
    public static String getText(@NonNull View view) {
        CharSequence txt = getTextRaw(view);
        if (txt == null)
            return null;
        return txt.toString();
    }

    public static void setText(@NonNull View view, @Nullable String text) {
        setTextRaw(view, text);
    }

    @Nullable
    public static CharSequence getTextRaw(@NonNull View view) {
        return ((TextView) view).getText();
    }

    public static void setTextRaw(@NonNull View view, @Nullable CharSequence text) {
        ((TextView) view).setText(text);
    }
}
