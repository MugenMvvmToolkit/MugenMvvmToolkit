package com.mugen.mvvm.interfaces.views;

public interface IToolbarView extends IAndroidView, IHasMenuView {
    CharSequence getTitle();

    void setTitle(CharSequence title);

    CharSequence getSubtitle();

    void setSubtitle(CharSequence subtitle);
}
