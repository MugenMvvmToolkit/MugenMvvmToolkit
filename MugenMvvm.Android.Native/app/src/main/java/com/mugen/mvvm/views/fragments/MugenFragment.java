package com.mugen.mvvm.views.fragments;

import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.Menu;
import android.view.MenuInflater;
import android.view.MenuItem;
import android.view.View;
import android.view.ViewGroup;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;

import com.mugen.mvvm.constants.LifecycleState;
import com.mugen.mvvm.interfaces.views.INativeFragmentView;
import com.mugen.mvvm.views.LifecycleExtensions;
import com.mugen.mvvm.views.ViewExtensions;

public class MugenFragment extends Fragment implements INativeFragmentView {
    private Object _state;

    @Override
    public Object getFragment() {
        return this;
    }

    @Override
    public int getViewId() {
        return ViewExtensions.tryGetViewId(getClass(), null, 0);
    }

    @Override
    public Object getState() {
        return _state;
    }

    @Override
    public void setState(Object tag) {
        _state = tag;
    }

    @Override
    public void onCreate(@Nullable Bundle savedInstanceState) {
        if (LifecycleExtensions.onLifecycleChanging(this, LifecycleState.Create, savedInstanceState)) {
            super.onCreate(savedInstanceState);
            LifecycleExtensions.onLifecycleChanged(this, LifecycleState.Create, savedInstanceState);
        }
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
            ViewExtensions.onSettingView(this, view);
            ViewExtensions.onSetView(this, view);
        }
        return view;
    }

    @Override
    public void onDestroyView() {
        View view = getView();
        if (view != null)
            ViewExtensions.onDestroyView(view);
        super.onDestroyView();
    }

    @Override
    public void onDestroy() {
        if (LifecycleExtensions.onLifecycleChanging(this, LifecycleState.Destroy, null)) {
            super.onDestroy();
            LifecycleExtensions.onLifecycleChanged(this, LifecycleState.Destroy, null);
            if (_state != null)
                _state = null;
        }
    }

    @Override
    public void onPause() {
        if (LifecycleExtensions.onLifecycleChanging(this, LifecycleState.Pause, null)) {
            super.onPause();
            LifecycleExtensions.onLifecycleChanged(this, LifecycleState.Pause, null);
        }
    }

    @Override
    public void onResume() {
        if (LifecycleExtensions.onLifecycleChanging(this, LifecycleState.Resume, null)) {
            super.onResume();
            LifecycleExtensions.onLifecycleChanged(this, LifecycleState.Resume, null);
        }
    }

    @Override
    public void onSaveInstanceState(@NonNull Bundle outState) {
        if (LifecycleExtensions.onLifecycleChanging(this, LifecycleState.SaveState, outState)) {
            super.onSaveInstanceState(outState);
            LifecycleExtensions.onLifecycleChanged(this, LifecycleState.SaveState, outState);
        }
    }

    @Override
    public void onCreateOptionsMenu(@NonNull Menu menu, @NonNull MenuInflater inflater) {
        if (LifecycleExtensions.onLifecycleChanging(this, LifecycleState.CreateOptionsMenu, menu)) {
            super.onCreateOptionsMenu(menu, inflater);
            LifecycleExtensions.onLifecycleChanged(this, LifecycleState.CreateOptionsMenu, menu);
        }
    }

    @Override
    public boolean onOptionsItemSelected(@NonNull MenuItem item) {
        if (LifecycleExtensions.onLifecycleChanging(this, LifecycleState.OptionsItemSelected, item)) {
            boolean result = super.onOptionsItemSelected(item);
            LifecycleExtensions.onLifecycleChanged(this, LifecycleState.OptionsItemSelected, item);
            return result;
        }
        return false;
    }

    @Override
    public void onStart() {
        if (LifecycleExtensions.onLifecycleChanging(this, LifecycleState.Start, null)) {
            super.onStart();
            LifecycleExtensions.onLifecycleChanged(this, LifecycleState.Start, null);
        }
    }

    @Override
    public void onStop() {
        if (LifecycleExtensions.onLifecycleChanging(this, LifecycleState.Stop, null)) {
            super.onStop();
            LifecycleExtensions.onLifecycleChanged(this, LifecycleState.Stop, null);
        }
    }
}
