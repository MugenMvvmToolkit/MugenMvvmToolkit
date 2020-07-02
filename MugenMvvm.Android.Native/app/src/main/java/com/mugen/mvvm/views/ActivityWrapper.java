package com.mugen.mvvm.views;

import android.app.Activity;
import com.mugen.mvvm.interfaces.views.IActivityView;
import com.mugen.mvvm.models.WeakTargetBase;

public class ActivityWrapper extends WeakTargetBase implements IActivityView {
    public ActivityWrapper(Object target) {
        super(target);
    }

    @Override
    public boolean isFinishing() {
        Activity target = (Activity) getTarget();
        if (target == null)
            return true;
        return target.isFinishing();
    }

    @Override
    public void finish() {
        Activity target = (Activity) getTarget();
        if (target != null)
            target.finish();
    }

    @Override
    public Object getTag(int id) {
        MugenActivity target = (MugenActivity) getTarget();
        if (target == null)
            return null;
        return target.getTag(id);
    }

    @Override
    public void setTag(int id, Object state) {
        MugenActivity target = (MugenActivity) getTarget();
        if (target != null)
            target.setTag(id, state);
    }

    @Override
    public int getViewId() {
        MugenActivity target = (MugenActivity) getTarget();
        if (target == null)
            return 0;
        return target.getViewId();
    }
}
