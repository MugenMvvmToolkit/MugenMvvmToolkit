package com.mugen.mvvm.views.support;

import android.view.Menu;
import android.view.View;
import androidx.appcompat.widget.Toolbar;
import com.mugen.mvvm.views.ViewExtensions;

public abstract class ToolbarCompatExtensions extends ViewExtensions {
    private static boolean _supported;

    public static boolean isSupported(View view) {
        return _supported && view instanceof Toolbar;
    }

    public static void setSupported() {
        _supported = true;
    }

    public static Menu getMenu(View view) {
        return ((Toolbar) view).getMenu();
    }

    public static CharSequence getTitle(View view) {
        return ((Toolbar) view).getTitle();
    }

    public static void setTitle(View view, CharSequence title) {
        ((Toolbar) view).setTitle(title);
    }

    public static CharSequence getSubtitle(View view) {
        return ((Toolbar) view).getSubtitle();
    }

    public static void setSubtitle(View view, CharSequence subtitle) {
        ((Toolbar) view).setSubtitle(subtitle);
    }
}
