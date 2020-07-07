package com.mugen.mvvm.views.support;

import android.view.Menu;
import androidx.appcompat.widget.Toolbar;
import com.mugen.mvvm.interfaces.views.IToolbarView;
import com.mugen.mvvm.views.ViewWrapper;

public class ToolbarCompatWrapper extends ViewWrapper implements IToolbarView {
    public ToolbarCompatWrapper(Object view) {
        super(view);
    }

    @Override
    public Menu getMenu() {
        Toolbar view = (Toolbar) getView();
        if (view == null)
            return null;
        return view.getMenu();
    }

    @Override
    public CharSequence getTitle() {
        Toolbar view = (Toolbar) getView();
        if (view == null)
            return null;
        return view.getTitle();
    }

    @Override
    public void setTitle(CharSequence title) {
        Toolbar view = (Toolbar) getView();
        if (view != null)
            view.setTitle(title);
    }

    @Override
    public CharSequence getSubtitle() {
        Toolbar view = (Toolbar) getView();
        if (view == null)
            return null;
        return view.getSubtitle();
    }

    @Override
    public void setSubtitle(CharSequence subtitle) {
        Toolbar view = (Toolbar) getView();
        if (view != null)
            view.setSubtitle(subtitle);
    }
}
