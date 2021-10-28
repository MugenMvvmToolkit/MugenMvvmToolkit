package com.mugen.mvvm.internal;

import android.app.Activity;
import android.view.View;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.mugen.mvvm.MugenService;
import com.mugen.mvvm.MugenUtils;
import com.mugen.mvvm.constants.PriorityConstant;
import com.mugen.mvvm.interfaces.IAsyncAppInitializer;
import com.mugen.mvvm.interfaces.ILifecycleDispatcher;
import com.mugen.mvvm.interfaces.views.IActivityView;
import com.mugen.mvvm.interfaces.views.IBindViewCallback;
import com.mugen.mvvm.interfaces.views.IViewAttributeAccessor;
import com.mugen.mvvm.views.ActivityMugenExtensions;

import java.util.ArrayList;

public class AsyncAppInitializer implements IAsyncAppInitializer, ILifecycleDispatcher, IBindViewCallback, IViewAttributeAccessor {
    protected static final int SetViewState = -1;
    protected static final int BindState = -2;

    protected final ArrayList<State> PendingStates;
    protected BindViewDispatcher BindDispatcher;
    protected IBindViewCallback BindCallback;
    protected ILifecycleDispatcher LifecycleDispatcher;
    protected State BindPendingState;

    public AsyncAppInitializer() {
        PendingStates = new ArrayList<>();
    }

    @Override
    public void setViewAccessor(@Nullable IViewAttributeAccessor accessor) {
    }

    @Override
    public void onSetView(@NonNull Object owner, @NonNull Object view) {
        PendingStates.add(new State(view, SetViewState, owner, false));
    }

    @Override
    public void bind(@NonNull Object view) {
        State state = new State(view, BindState, BindViewDispatcher.AttributeAccessor.getBind(), false);
        state.BindStyle = BindViewDispatcher.AttributeAccessor.getBindStyle();
        state.ItemTemplate = BindViewDispatcher.AttributeAccessor.getItemTemplate();
        PendingStates.add(state);
    }

    @Nullable
    @Override
    public String getString(int index) {
        throw new UnsupportedOperationException();
    }

    @Override
    public int getResourceId(int index) {
        throw new UnsupportedOperationException();
    }

    @Nullable
    @Override
    public String getBind() {
        if (BindPendingState == null)
            return null;
        return (String) BindPendingState.State;
    }

    @Nullable
    @Override
    public String getBindStyle() {
        if (BindPendingState == null)
            return null;
        return BindPendingState.BindStyle;
    }

    @Override
    public int getItemTemplate() {
        if (BindPendingState == null)
            return 0;
        return BindPendingState.ItemTemplate;
    }

    @Override
    public boolean onLifecycleChanging(@NonNull Object target, int lifecycle, @Nullable Object state, boolean cancelable) {
        if (!isFinishing(target))
            PendingStates.add(new State(target, lifecycle, state, true));
        return true;
    }

    @Override
    public void onLifecycleChanged(@NonNull Object target, int lifecycle, @Nullable Object state) {
        if (!isFinishing(target))
            PendingStates.add(new State(target, lifecycle, state, false));
    }

    @Override
    public void onInitializationStarted() {
        MugenService.addLifecycleDispatcher(this, false);
        BindDispatcher = new BindViewDispatcher(this);
        MugenService.addViewDispatcher(BindDispatcher);
    }

    @Override
    public void onInitializationCompleted() {
        if (BindCallback == null || LifecycleDispatcher == null)
            throw new UnsupportedOperationException("MugenUtils.initialize was not called");
        BindCallback.setViewAccessor(this);
        for (int i = 0; i < PendingStates.size(); i++) {
            State state = PendingStates.get(i);
            if (isFinishing(state.Target))
                continue;

            if (state.Lifecycle == SetViewState)
                BindCallback.onSetView(state.State, state.Target);
            else if (state.Lifecycle == BindState) {
                BindPendingState = state;
                BindCallback.bind(state.Target);
            } else if (state.IsChanging)
                LifecycleDispatcher.onLifecycleChanging(state.Target, state.Lifecycle, state.State, false);
            else
                LifecycleDispatcher.onLifecycleChanged(state.Target, state.Lifecycle, state.State);
        }

        MugenService.removeLifecycleDispatcher(this);
        MugenService.addLifecycleDispatcher(LifecycleDispatcher, false);
        BindDispatcher.setBindCallback(BindCallback);
    }

    @Override
    public void initialize(@NonNull IBindViewCallback bindCallback, @NonNull ILifecycleDispatcher lifecycleDispatcher) {
        BindCallback = bindCallback;
        LifecycleDispatcher = MugenUtils.isNativeMode() ? new NativeLifecycleDispatcherWrapper(lifecycleDispatcher) : lifecycleDispatcher;
    }

    @Override
    public int getPriority() {
        return PriorityConstant.Default;
    }

    protected static boolean isFinishing(Object view) {
        if (view instanceof IActivityView)
            return ((IActivityView) view).isFinishing();
        if (view instanceof View) {
            Activity activity = (Activity) ActivityMugenExtensions.tryGetActivity(((View) view).getContext());
            if (activity == null)
                return false;
            return activity.isFinishing();
        }
        return false;
    }

    protected static class State {
        public final Object Target;
        public final int Lifecycle;
        public final Object State;
        public final boolean IsChanging;
        public String BindStyle;
        public int ItemTemplate;

        public State(Object target, int lifecycle, Object state, boolean isChanging) {
            Target = target;
            Lifecycle = lifecycle;
            State = state;
            IsChanging = isChanging;
        }
    }
}
