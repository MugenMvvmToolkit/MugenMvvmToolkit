package com.mugen.mvvm.views.fragments;

import android.content.DialogInterface;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.Menu;
import android.view.MenuInflater;
import android.view.MenuItem;
import android.view.View;
import android.view.ViewGroup;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.DialogFragment;

import com.mugen.mvvm.constants.LifecycleState;
import com.mugen.mvvm.interfaces.views.IDialogFragmentView;
import com.mugen.mvvm.interfaces.views.INativeFragmentView;
import com.mugen.mvvm.views.LifecycleMugenExtensions;
import com.mugen.mvvm.views.ViewMugenExtensions;

public class MugenDialogFragment extends DialogFragment implements INativeFragmentView, IDialogFragmentView {
    private Object _state;
    private int _viewId;

    @NonNull
    @Override
    public Object getFragment() {
        return this;
    }

    @Override
    public void setViewResourceId(int resourceId) {
        _viewId = resourceId;
    }

    @Override
    public int getViewId() {
        if (_viewId != 0)
            return _viewId;
        return ViewMugenExtensions.tryGetLayoutId(getClass(), null, 0, null);
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
    public void onCreate(@Nullable Bundle savedInstanceState) {
        LifecycleMugenExtensions.onLifecycleChanging(this, LifecycleState.Create, savedInstanceState, false);
        super.onCreate(savedInstanceState);
        LifecycleMugenExtensions.onLifecycleChanged(this, LifecycleState.Create, savedInstanceState);
    }

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        int viewId = getViewId();
        View view;
        if (viewId == 0)
            view = super.onCreateView(inflater, container, savedInstanceState);
        else
            view = inflater.inflate(viewId, container, false);
        if (view != null) {
            ViewMugenExtensions.onInitializingView(this, view);
            ViewMugenExtensions.onInitializedView(this, view);
        }
        return view;
    }

    @Override
    public void onDestroyView() {
        View view = getView();
        if (view != null)
            ViewMugenExtensions.onDestroyView(view);
        super.onDestroyView();
    }

    @Override
    public void onDestroy() {
        LifecycleMugenExtensions.onLifecycleChanging(this, LifecycleState.Destroy, null, false);
        super.onDestroy();
        LifecycleMugenExtensions.onLifecycleChanged(this, LifecycleState.Destroy, null);
        if (_state != null)
            _state = null;

    }

    @Override
    public void onPause() {
        LifecycleMugenExtensions.onLifecycleChanging(this, LifecycleState.Pause, null, false);
        super.onPause();
        LifecycleMugenExtensions.onLifecycleChanged(this, LifecycleState.Pause, null);
    }

    @Override
    public void onResume() {
        LifecycleMugenExtensions.onLifecycleChanging(this, LifecycleState.Resume, null, false);
        super.onResume();
        LifecycleMugenExtensions.onLifecycleChanged(this, LifecycleState.Resume, null);
    }

    @Override
    public void onSaveInstanceState(@NonNull Bundle outState) {
        LifecycleMugenExtensions.onLifecycleChanging(this, LifecycleState.SaveState, outState, false);
        super.onSaveInstanceState(outState);
        LifecycleMugenExtensions.onLifecycleChanged(this, LifecycleState.SaveState, outState);
    }

    @Override
    public void onCreateOptionsMenu(@NonNull Menu menu, @NonNull MenuInflater inflater) {
        if (LifecycleMugenExtensions.onLifecycleChanging(this, LifecycleState.CreateOptionsMenu, menu, true)) {
            super.onCreateOptionsMenu(menu, inflater);
            LifecycleMugenExtensions.onLifecycleChanged(this, LifecycleState.CreateOptionsMenu, menu);
        }
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
    public void onStart() {
        LifecycleMugenExtensions.onLifecycleChanging(this, LifecycleState.Start, null, false);
        super.onStart();
        LifecycleMugenExtensions.onLifecycleChanged(this, LifecycleState.Start, null);
    }

    @Override
    public void onStop() {
        LifecycleMugenExtensions.onLifecycleChanging(this, LifecycleState.Stop, null, false);
        super.onStop();
        LifecycleMugenExtensions.onLifecycleChanged(this, LifecycleState.Stop, null);
    }

    @Override
    public void dismiss() {
        if (LifecycleMugenExtensions.onLifecycleChanging(this, LifecycleState.Dismiss, null, true)) {
            super.dismiss();
            LifecycleMugenExtensions.onLifecycleChanged(this, LifecycleState.Dismiss, null);
        }
    }

    @Override
    public void dismissAllowingStateLoss() {
        if (LifecycleMugenExtensions.onLifecycleChanging(this, LifecycleState.DismissAllowingStateLoss, null, true)) {
            super.dismissAllowingStateLoss();
            LifecycleMugenExtensions.onLifecycleChanged(this, LifecycleState.DismissAllowingStateLoss, null);
        }
    }

    @Override
    public void onCancel(@NonNull DialogInterface dialog) {
        if (LifecycleMugenExtensions.onLifecycleChanging(this, LifecycleState.Cancel, dialog, true)) {
            super.onCancel(dialog);
            LifecycleMugenExtensions.onLifecycleChanged(this, LifecycleState.Cancel, dialog);
        }
    }
}
