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
import com.mugen.mvvm.internal.MugenService;
import com.mugen.mvvm.constants.LifecycleState;
import com.mugen.mvvm.extensions.MugenExtensions;
import com.mugen.mvvm.internal.MugenContextWrapper;
import com.mugen.mvvm.views.support.MugenAppCompatActivity;

public class MugenActivity extends Activity implements INativeActivityView {
    private SparseArray<Object> _state;

    @Override
    public Activity getActivity() {
        return this;
    }

    @Override
    public int getViewId() {
        return MugenExtensions.tryGetViewId(getClass(), getIntent(), 0);
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
        if (MugenService.onLifecycleChanging(this, LifecycleState.Finish, null)) {
            super.finish();
            MugenService.onLifecycleChanged(this, LifecycleState.Finish, null);
        }
    }

    @Override
    public void finishAfterTransition() {
        if (MugenService.onLifecycleChanging(this, LifecycleState.FinishAfterTransition, null)) {
            super.finishAfterTransition();
            MugenService.onLifecycleChanged(this, LifecycleState.FinishAfterTransition, null);
        }
    }

    @Override
    public void onBackPressed() {
        if (MugenService.onLifecycleChanging(this, LifecycleState.BackPressed, null)) {
            super.onBackPressed();
            MugenService.onLifecycleChanged(this, LifecycleState.BackPressed, null);
        }
    }

    @Override
    protected void onNewIntent(Intent intent) {
        if (MugenService.onLifecycleChanging(this, LifecycleState.NewIntent, intent)) {
            super.onNewIntent(intent);
            MugenService.onLifecycleChanged(this, LifecycleState.NewIntent, intent);
        }
    }

    @Override
    public void onConfigurationChanged(Configuration newConfig) {
        if (MugenService.onLifecycleChanging(this, LifecycleState.ConfigurationChanged, newConfig)) {
            super.onConfigurationChanged(newConfig);
            MugenService.onLifecycleChanged(this, LifecycleState.ConfigurationChanged, newConfig);
        }
    }

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        if (MugenService.onLifecycleChanging(this, LifecycleState.Create, savedInstanceState)) {
            super.onCreate(savedInstanceState);
            MugenService.onLifecycleChanged(this, LifecycleState.Create, savedInstanceState);
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
        super.setContentView(view);
        MugenService.onSetView(this, view);
    }

    @Override
    public void setContentView(View view, ViewGroup.LayoutParams params) {
        super.setContentView(view, params);
        MugenService.onSetView(this, view);
    }

    @Override
    protected void onDestroy() {
        if (MugenService.onLifecycleChanging(this, LifecycleState.Destroy, null)) {
            super.onDestroy();
            MugenService.onLifecycleChanged(this, LifecycleState.Destroy, null);
            if (_state != null)
                _state = null;
        }
    }

    @Override
    protected void onPause() {
        if (MugenService.onLifecycleChanging(this, LifecycleState.Pause, null)) {
            super.onPause();
            MugenService.onLifecycleChanged(this, LifecycleState.Pause, null);
        }
    }

    @Override
    protected void onRestart() {
        if (MugenService.onLifecycleChanging(this, LifecycleState.Restart, null)) {
            super.onRestart();
            MugenService.onLifecycleChanged(this, LifecycleState.Restart, null);
        }
    }

    @Override
    protected void onResume() {
        if (MugenService.onLifecycleChanging(this, LifecycleState.Resume, null)) {
            super.onResume();
            MugenService.onLifecycleChanged(this, LifecycleState.Resume, null);
        }
    }

    @Override
    protected void onSaveInstanceState(Bundle outState) {
        if (MugenService.onLifecycleChanging(this, LifecycleState.SaveState, outState)) {
            super.onSaveInstanceState(outState);
            MugenService.onLifecycleChanged(this, LifecycleState.SaveState, outState);
        }
    }

    @Override
    protected void onStart() {
        if (MugenService.onLifecycleChanging(this, LifecycleState.Start, null)) {
            super.onStart();
            MugenService.onLifecycleChanged(this, LifecycleState.Start, null);
        }
    }

    @Override
    protected void onStop() {
        if (MugenService.onLifecycleChanging(this, LifecycleState.Stop, null)) {
            super.onStop();
            MugenService.onLifecycleChanged(this, LifecycleState.Stop, null);
        }
    }

    @Override
    protected void onPostCreate(Bundle savedInstanceState) {
        if (MugenService.onLifecycleChanging(this, LifecycleState.PostCreate, savedInstanceState)) {
            super.onPostCreate(savedInstanceState);
            MugenService.onLifecycleChanged(this, LifecycleState.PostCreate, savedInstanceState);
        }
    }

    public static class Main extends MugenAppCompatActivity {
    }
}
