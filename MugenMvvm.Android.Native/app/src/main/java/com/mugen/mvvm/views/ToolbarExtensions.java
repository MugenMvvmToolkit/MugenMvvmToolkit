package com.mugen.mvvm.views;

import android.os.Build;
import android.view.Menu;
import android.view.View;
import android.widget.Toolbar;
import androidx.annotation.RequiresApi;

public abstract class ToolbarExtensions extends ViewExtensions {
    public static boolean isSupported(View view){
        return Build.VERSION.SDK_INT >= Build.VERSION_CODES.LOLLIPOP && view instanceof Toolbar;
    }

    @RequiresApi(api = Build.VERSION_CODES.LOLLIPOP)
    public static Menu getMenu(View view) {
        return ((Toolbar) view).getMenu();
    }

    @RequiresApi(api = Build.VERSION_CODES.LOLLIPOP)
    public static CharSequence getTitle(View view) {
        return ((Toolbar) view).getTitle();
    }

    @RequiresApi(api = Build.VERSION_CODES.LOLLIPOP)
    public static void setTitle(View view, CharSequence title) {
        ((Toolbar) view).setTitle(title);
    }

    @RequiresApi(api = Build.VERSION_CODES.LOLLIPOP)
    public static CharSequence getSubtitle(View view) {
        return ((Toolbar) view).getSubtitle();
    }

    @RequiresApi(api = Build.VERSION_CODES.LOLLIPOP)
    public static void setSubtitle(View view, CharSequence subtitle) {
        ((Toolbar) view).setSubtitle(subtitle);
    }
}
