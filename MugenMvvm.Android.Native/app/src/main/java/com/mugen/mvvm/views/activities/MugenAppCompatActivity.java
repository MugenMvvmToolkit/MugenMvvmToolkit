package com.mugen.mvvm.views.activities;

import android.content.Context;
import android.content.Intent;
import android.content.res.Configuration;
import android.os.Bundle;
import android.util.AttributeSet;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.view.ViewGroup;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.appcompat.app.AppCompatActivity;

import com.mugen.mvvm.constants.LifecycleState;
import com.mugen.mvvm.interfaces.views.INativeActivityView;
import com.mugen.mvvm.internal.MugenContextWrapper;
import com.mugen.mvvm.views.LifecycleMugenExtensions;
import com.mugen.mvvm.views.ViewMugenExtensions;

public class MugenAppCompatActivity extends AppCompatActivity implements INativeActivityView {
    private Object _state;

    @NonNull
    @Override
    public Object getActivity() {
        return this;
    }

    @Override
    public int getViewId() {
        return ViewMugenExtensions.tryGetLayoutId(getClass(), getIntent(), 0, null);
    }

    @Nullable
    @Override
    public Object getState() {
        return _state;
    }

    @Override
    public void setState(@Nullable Object tag) {
        _state = tag;
    }

    @Override
    public Context getContext() {
        return this;
    }

    @Override
    protected void attachBaseContext(Context newBase) {
        super.attachBaseContext(MugenContextWrapper.wrap(newBase));
    }

    @Override
    public View onCreateView(View parent, @NonNull String name, @NonNull Context context, @NonNull AttributeSet attrs) {
        return MugenContextWrapper.onActivityCreateView(this, parent, super.onCreateView(parent, name, context, attrs), name, context, attrs);
    }

    @Override
    public void finish() {
        if (LifecycleMugenExtensions.onLifecycleChanging(this, LifecycleState.Finish, null, true)) {
            super.finish();
            LifecycleMugenExtensions.onLifecycleChanged(this, LifecycleState.Finish, null);
        }
    }

    @Override
    public void finishAfterTransition() {
        if (LifecycleMugenExtensions.onLifecycleChanging(this, LifecycleState.FinishAfterTransition, null, true)) {
            super.finishAfterTransition();
            LifecycleMugenExtensions.onLifecycleChanged(this, LifecycleState.FinishAfterTransition, null);
        }
    }

    @Override
    public void onBackPressed() {
        if (LifecycleMugenExtensions.onLifecycleChanging(this, LifecycleState.BackPressed, null, true)) {
            super.onBackPressed();
            LifecycleMugenExtensions.onLifecycleChanged(this, LifecycleState.BackPressed, null);
        }
    }

    @Override
    protected void onNewIntent(Intent intent) {
        LifecycleMugenExtensions.onLifecycleChanging(this, LifecycleState.NewIntent, intent, false);
        super.onNewIntent(intent);
        LifecycleMugenExtensions.onLifecycleChanged(this, LifecycleState.NewIntent, intent);
    }

    @Override
    public void onConfigurationChanged(@NonNull Configuration newConfig) {
        LifecycleMugenExtensions.onLifecycleChanging(this, LifecycleState.ConfigurationChanged, newConfig, false);
        super.onConfigurationChanged(newConfig);
        LifecycleMugenExtensions.onLifecycleChanged(this, LifecycleState.ConfigurationChanged, newConfig);
    }

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        LifecycleMugenExtensions.onLifecycleChanging(this, LifecycleState.Create, savedInstanceState, false);
        super.onCreate(savedInstanceState);
        LifecycleMugenExtensions.onLifecycleChanged(this, LifecycleState.Create, savedInstanceState);
        if (isFinishing())
            return;
        int viewId = getViewId();
        if (viewId != 0)
            setContentView(viewId);
    }

    @Override
    public void setContentView(int layoutResID) {
        if (!isFinishing())
            setContentView(getLayoutInflater().inflate(layoutResID, null));
    }

    @Override
    public void setContentView(View view) {
        if (isFinishing())
            return;
        ViewMugenExtensions.onInitializingView(this, view);
        super.setContentView(view);
        ViewMugenExtensions.onInitializedView(this, view);
    }

    @Override
    public void setContentView(View view, ViewGroup.LayoutParams params) {
        if (isFinishing())
            return;
        ViewMugenExtensions.onInitializingView(this, view);
        super.setContentView(view, params);
        ViewMugenExtensions.onInitializedView(this, view);
    }

    @Override
    protected void onDestroy() {
        LifecycleMugenExtensions.onLifecycleChanging(this, LifecycleState.Destroy, null, false);
        super.onDestroy();
        LifecycleMugenExtensions.onLifecycleChanged(this, LifecycleState.Destroy, null);
        if (_state != null)
            _state = null;
    }

    @Override
    protected void onPause() {
        LifecycleMugenExtensions.onLifecycleChanging(this, LifecycleState.Pause, null, false);
        super.onPause();
        LifecycleMugenExtensions.onLifecycleChanged(this, LifecycleState.Pause, null);
    }

    @Override
    protected void onRestart() {
        LifecycleMugenExtensions.onLifecycleChanging(this, LifecycleState.Restart, null, false);
        super.onRestart();
        LifecycleMugenExtensions.onLifecycleChanged(this, LifecycleState.Restart, null);
    }

    @Override
    protected void onResume() {
        LifecycleMugenExtensions.onLifecycleChanging(this, LifecycleState.Resume, null, false);
        super.onResume();
        LifecycleMugenExtensions.onLifecycleChanged(this, LifecycleState.Resume, null);
    }

    @Override
    protected void onSaveInstanceState(@NonNull Bundle outState) {
        LifecycleMugenExtensions.onLifecycleChanging(this, LifecycleState.SaveState, outState, false);
        super.onSaveInstanceState(outState);
        LifecycleMugenExtensions.onLifecycleChanged(this, LifecycleState.SaveState, outState);
    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        if (LifecycleMugenExtensions.onLifecycleChanging(this, LifecycleState.CreateOptionsMenu, menu, true)) {
            boolean result = super.onCreateOptionsMenu(menu);
            LifecycleMugenExtensions.onLifecycleChanged(this, LifecycleState.CreateOptionsMenu, menu);
            return result;
        }
        return false;
    }

    @Override
    public boolean onOptionsItemSelected(@NonNull MenuItem item) {
        if (LifecycleMugenExtensions.onLifecycleChanging(this, LifecycleState.OptionsItemSelected, item, true)) {
            boolean result = super.onOptionsItemSelected(item);
            LifecycleMugenExtensions.onLifecycleChanged(this, LifecycleState.OptionsItemSelected, item);
            return result;
        }
        return false;
    }

    @Override
    protected void onStart() {
        LifecycleMugenExtensions.onLifecycleChanging(this, LifecycleState.Start, null, false);
        super.onStart();
        LifecycleMugenExtensions.onLifecycleChanged(this, LifecycleState.Start, null);
    }

    @Override
    protected void onStop() {
        LifecycleMugenExtensions.onLifecycleChanging(this, LifecycleState.Stop, null, false);
        super.onStop();
        LifecycleMugenExtensions.onLifecycleChanged(this, LifecycleState.Stop, null);
    }

    @Override
    protected void onPostCreate(Bundle savedInstanceState) {
        LifecycleMugenExtensions.onLifecycleChanging(this, LifecycleState.PostCreate, savedInstanceState, false);
        super.onPostCreate(savedInstanceState);
        LifecycleMugenExtensions.onLifecycleChanged(this, LifecycleState.PostCreate, savedInstanceState);
    }
}
