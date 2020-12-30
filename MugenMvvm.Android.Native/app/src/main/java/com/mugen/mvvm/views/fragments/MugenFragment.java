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
import com.mugen.mvvm.views.LifecycleMugenExtensions;
import com.mugen.mvvm.views.ViewMugenExtensions;

public class MugenFragment extends Fragment implements INativeFragmentView {
    private Object _state;

    @Override
    public Object getFragment() {
        return this;
    }

    @Override
    public int getViewId() {
        return ViewMugenExtensions.tryGetViewId(getClass(), null, 0);
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
        if (LifecycleMugenExtensions.onLifecycleChanging(this, LifecycleState.Create, savedInstanceState)) {
            super.onCreate(savedInstanceState);
            LifecycleMugenExtensions.onLifecycleChanged(this, LifecycleState.Create, savedInstanceState);
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
        if (LifecycleMugenExtensions.onLifecycleChanging(this, LifecycleState.Destroy, null)) {
            super.onDestroy();
            LifecycleMugenExtensions.onLifecycleChanged(this, LifecycleState.Destroy, null);
            if (_state != null)
                _state = null;
        }
    }

    @Override
    public void onPause() {
        if (LifecycleMugenExtensions.onLifecycleChanging(this, LifecycleState.Pause, null)) {
            super.onPause();
            LifecycleMugenExtensions.onLifecycleChanged(this, LifecycleState.Pause, null);
        }
    }

    @Override
    public void onResume() {
        if (LifecycleMugenExtensions.onLifecycleChanging(this, LifecycleState.Resume, null)) {
            super.onResume();
            LifecycleMugenExtensions.onLifecycleChanged(this, LifecycleState.Resume, null);
        }
    }

    @Override
    public void onSaveInstanceState(@NonNull Bundle outState) {
        if (LifecycleMugenExtensions.onLifecycleChanging(this, LifecycleState.SaveState, outState)) {
            super.onSaveInstanceState(outState);
            LifecycleMugenExtensions.onLifecycleChanged(this, LifecycleState.SaveState, outState);
        }
    }

    @Override
    public void onCreateOptionsMenu(@NonNull Menu menu, @NonNull MenuInflater inflater) {
        if (LifecycleMugenExtensions.onLifecycleChanging(this, LifecycleState.CreateOptionsMenu, menu)) {
            super.onCreateOptionsMenu(menu, inflater);
            LifecycleMugenExtensions.onLifecycleChanged(this, LifecycleState.CreateOptionsMenu, menu);
        }
    }

    @Override
    public boolean onOptionsItemSelected(@NonNull MenuItem item) {
        if (LifecycleMugenExtensions.onLifecycleChanging(this, LifecycleState.OptionsItemSelected, item)) {
            boolean result = super.onOptionsItemSelected(item);
            LifecycleMugenExtensions.onLifecycleChanged(this, LifecycleState.OptionsItemSelected, item);
            return result;
        }
        return false;
    }

    @Override
    public void onStart() {
        if (LifecycleMugenExtensions.onLifecycleChanging(this, LifecycleState.Start, null)) {
            super.onStart();
            LifecycleMugenExtensions.onLifecycleChanged(this, LifecycleState.Start, null);
        }
    }

    @Override
    public void onStop() {
        if (LifecycleMugenExtensions.onLifecycleChanging(this, LifecycleState.Stop, null)) {
            super.onStop();
            LifecycleMugenExtensions.onLifecycleChanged(this, LifecycleState.Stop, null);
        }
    }
}
