package com.mugen.mvvm.views;

import android.app.Activity;
import com.mugen.mvvm.interfaces.views.IActivityView;
import com.mugen.mvvm.interfaces.views.IHasTagView;
import com.mugen.mvvm.interfaces.views.IResourceView;
import com.mugen.mvvm.models.WeakTargetBase;

public class ActivityWrapper extends WeakTargetBase<Activity> implements IActivityView {
    private boolean _isFinishing;

    public ActivityWrapper(Object target) {
        super((Activity) target);
    }

    @Override
    public Activity getActivity() {
        return getTarget();
    }

    @Override
    public boolean isFinishing() {
        Activity target = getTarget();
        if (target == null)
            return _isFinishing;
        return target.isFinishing();
    }

    @Override
    public void finish() {
        Activity target = getTarget();
        if (target != null)
            target.finish();
    }

    @Override
    public Object getTag(int id) {
        IHasTagView target = (IHasTagView) getTarget();
        if (target == null)
            return null;
        return target.getTag(id);
    }

    @Override
    public void setTag(int id, Object state) {
        IHasTagView target = (IHasTagView) getTarget();
        if (target != null)
            target.setTag(id, state);
    }

    @Override
    public int getViewId() {
        IResourceView target = (IResourceView) getTarget();
        if (target == null)
            return 0;
        return target.getViewId();
    }

    @Override
    protected void onReleased(Activity target) {
        _isFinishing = target.isFinishing();
        super.onReleased(target);
    }
}
