package com.mugen.mvvm.views;

import android.view.View;
import android.widget.FrameLayout;
import com.mugen.mvvm.extensions.MugenExtensions;
import com.mugen.mvvm.interfaces.views.IAndroidView;
import com.mugen.mvvm.interfaces.views.IContentView;

public class ContentViewWrapper extends ViewWrapper implements IContentView {
    //todo handle fragments
    public ContentViewWrapper(Object view) {
        super(view);
    }

    @Override
    public Object getContent() {
        FrameLayout view = (FrameLayout) getView();
        if (view == null)
            return null;
        if (view.getChildCount() == 0)
            return null;
        return MugenExtensions.wrap(view.getChildAt(0), true);
    }

    @Override
    public void setContent(Object content) {
        FrameLayout view = (FrameLayout) getView();
        if (view == null)
            return;
        if (content instanceof IAndroidView)
            content = ((IAndroidView) content).getView();
        view.removeAllViews();
        if (content != null)
            view.addView((View) content);
    }
}
