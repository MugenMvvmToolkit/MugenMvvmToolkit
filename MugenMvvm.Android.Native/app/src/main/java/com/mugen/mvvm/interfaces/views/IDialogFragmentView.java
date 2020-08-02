package com.mugen.mvvm.interfaces.views;

public interface IDialogFragmentView extends IFragmentView {
    boolean isCancelable();

    void setCancelable(boolean cancelable);

    void dismiss();

    void dismissAllowingStateLoss();
}
