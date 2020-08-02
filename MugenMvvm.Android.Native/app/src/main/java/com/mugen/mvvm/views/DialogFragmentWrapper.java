package com.mugen.mvvm.views;

import com.mugen.mvvm.interfaces.views.IDialogFragmentView;
import com.mugen.mvvm.interfaces.views.INativeFragmentView;

public class DialogFragmentWrapper extends FragmentWrapper implements IDialogFragmentView {
    public DialogFragmentWrapper(INativeFragmentView target) {
        super(target);
    }

    @Override
    public boolean isCancelable() {
        return ((IDialogFragmentView) Target).isCancelable();
    }

    @Override
    public void setCancelable(boolean cancelable) {
        ((IDialogFragmentView) Target).setCancelable(cancelable);
    }

    @Override
    public void dismiss() {
        ((IDialogFragmentView) Target).dismiss();
    }

    @Override
    public void dismissAllowingStateLoss() {
        ((IDialogFragmentView) Target).dismissAllowingStateLoss();
    }
}
