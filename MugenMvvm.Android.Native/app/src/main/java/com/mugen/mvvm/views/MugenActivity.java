package com.mugen.mvvm.views;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.content.res.Configuration;
import android.os.Bundle;
import android.util.AttributeSet;
import android.util.SparseArray;
import android.view.View;
import android.view.ViewGroup;
import com.mugen.mvvm.interfaces.views.INativeActivityView;
import com.mugen.mvvm.constants.LifecycleState;
import com.mugen.mvvm.internal.MugenContextWrapper;

public class MugenActivity extends Activity implements INativeActivityView {
    private SparseArray<Object> _state;

    @Override
    public Context getActivity() {
        return this;
    }

    @Override
    public int getViewId() {
        return ViewExtensions.tryGetViewId(getClass(), getIntent(), 0);
    }

    @Override
    public Object getTag(int id) {
        if (_state != null)
            return _state.get(id);
        return null;
    }

    @Override
    public void setTag(int id, Object state) {
        if (_state == null) {
            _state = new SparseArray<Object>();
        }
        if (state == null)
            _state.remove(id);
        else
            _state.put(id, state);
    }

    @Override
    protected void attachBaseContext(Context newBase) {
        super.attachBaseContext(MugenContextWrapper.wrap(newBase));
    }

    @Override
    public View onCreateView(View parent, String name, Context context, AttributeSet attrs) {
        return MugenContextWrapper.onActivityCreateView(this, parent, super.onCreateView(parent, name, context, attrs), name, context, attrs);
    }

    @Override
    public void finish() {
        if (LifecycleExtensions.onLifecycleChanging(this, LifecycleState.Finish, null)) {
            super.finish();
            LifecycleExtensions.onLifecycleChanged(this, LifecycleState.Finish, null);
        }
    }

    @Override
    public void finishAfterTransition() {
        if (LifecycleExtensions.onLifecycleChanging(this, LifecycleState.FinishAfterTransition, null)) {
            super.finishAfterTransition();
            LifecycleExtensions.onLifecycleChanged(this, LifecycleState.FinishAfterTransition, null);
        }
    }

    @Override
    public void onBackPressed() {
        if (LifecycleExtensions.onLifecycleChanging(this, LifecycleState.BackPressed, null)) {
            super.onBackPressed();
            LifecycleExtensions.onLifecycleChanged(this, LifecycleState.BackPressed, null);
        }
    }

    @Override
    protected void onNewIntent(Intent intent) {
        if (LifecycleExtensions.onLifecycleChanging(this, LifecycleState.NewIntent, intent)) {
            super.onNewIntent(intent);
            LifecycleExtensions.onLifecycleChanged(this, LifecycleState.NewIntent, intent);
        }
    }

    @Override
    public void onConfigurationChanged(Configuration newConfig) {
        if (LifecycleExtensions.onLifecycleChanging(this, LifecycleState.ConfigurationChanged, newConfig)) {
            super.onConfigurationChanged(newConfig);
            LifecycleExtensions.onLifecycleChanged(this, LifecycleState.ConfigurationChanged, newConfig);
        }
    }

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        if (LifecycleExtensions.onLifecycleChanging(this, LifecycleState.Create, savedInstanceState)) {
            super.onCreate(savedInstanceState);
            LifecycleExtensions.onLifecycleChanged(this, LifecycleState.Create, savedInstanceState);
            if (isFinishing())
                return;
            int viewId = getViewId();
            if (viewId != 0)
                setContentView(viewId);
        }
    }

    @Override
    public void setContentView(int layoutResID) {
        setContentView(getLayoutInflater().inflate(layoutResID, null));
    }

    @Override
    public void setContentView(View view) {
        ViewExtensions.onSettingView(this, view);
        super.setContentView(view);
        ViewExtensions.onSetView(this, view);
    }

    @Override
    public void setContentView(View view, ViewGroup.LayoutParams params) {
        ViewExtensions.onSettingView(this, view);
        super.setContentView(view, params);
        ViewExtensions.onSetView(this, view);
    }

    @Override
    protected void onDestroy() {
        if (LifecycleExtensions.onLifecycleChanging(this, LifecycleState.Destroy, null)) {
            super.onDestroy();
            LifecycleExtensions.onLifecycleChanged(this, LifecycleState.Destroy, null);
            if (_state != null)
                _state = null;
        }
    }

    @Override
    protected void onPause() {
        if (LifecycleExtensions.onLifecycleChanging(this, LifecycleState.Pause, null)) {
            super.onPause();
            LifecycleExtensions.onLifecycleChanged(this, LifecycleState.Pause, null);
        }
    }

    @Override
    protected void onRestart() {
        if (LifecycleExtensions.onLifecycleChanging(this, LifecycleState.Restart, null)) {
            super.onRestart();
            LifecycleExtensions.onLifecycleChanged(this, LifecycleState.Restart, null);
        }
    }

    @Override
    protected void onResume() {
        if (LifecycleExtensions.onLifecycleChanging(this, LifecycleState.Resume, null)) {
            super.onResume();
            LifecycleExtensions.onLifecycleChanged(this, LifecycleState.Resume, null);
        }
    }

    @Override
    protected void onSaveInstanceState(Bundle outState) {
        if (LifecycleExtensions.onLifecycleChanging(this, LifecycleState.SaveState, outState)) {
            super.onSaveInstanceState(outState);
            LifecycleExtensions.onLifecycleChanged(this, LifecycleState.SaveState, outState);
        }
    }

    @Override
    protected void onStart() {
        if (LifecycleExtensions.onLifecycleChanging(this, LifecycleState.Start, null)) {
            super.onStart();
            LifecycleExtensions.onLifecycleChanged(this, LifecycleState.Start, null);
        }
    }

    @Override
    protected void onStop() {
        if (LifecycleExtensions.onLifecycleChanging(this, LifecycleState.Stop, null)) {
            super.onStop();
            LifecycleExtensions.onLifecycleChanged(this, LifecycleState.Stop, null);
        }
    }

    @Override
    protected void onPostCreate(Bundle savedInstanceState) {
        if (LifecycleExtensions.onLifecycleChanging(this, LifecycleState.PostCreate, savedInstanceState)) {
            super.onPostCreate(savedInstanceState);
            LifecycleExtensions.onLifecycleChanged(this, LifecycleState.PostCreate, savedInstanceState);
        }
    }
}
