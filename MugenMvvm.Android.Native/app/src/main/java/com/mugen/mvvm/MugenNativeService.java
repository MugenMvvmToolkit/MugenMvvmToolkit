package com.mugen.mvvm;

import android.content.Context;
import androidx.appcompat.widget.Toolbar;
import androidx.recyclerview.widget.RecyclerView;
import androidx.swiperefreshlayout.widget.SwipeRefreshLayout;
import com.mugen.mvvm.extensions.MugenExtensions;
import com.mugen.mvvm.interfaces.ILifecycleDispatcher;
import com.mugen.mvvm.interfaces.INativeWeakReferenceCallback;
import com.mugen.mvvm.interfaces.views.*;
import com.mugen.mvvm.internal.*;
import com.mugen.mvvm.views.MainMugenActivity;
import com.mugen.mvvm.views.MugenActivity;
import com.mugen.mvvm.views.support.*;

public final class MugenNativeService {
    private MugenNativeService() {
    }

    public static void initialize(Context context) {
        MugenService.initialize(context, false);
    }

    public static void initializeNative(Context context, IViewBindCallback bindCallback, INativeWeakReferenceCallback weakReferenceCallback) {
        MugenService.initialize(context, true);
        MugenService.setWeakReferenceCallback(weakReferenceCallback);
        MugenService.addViewDispatcher(new NativeViewDispatcher(bindCallback));
        MugenService.addLifecycleDispatcher(new FragmentStateCleaner());
        MugenService.addLifecycleDispatcher(new ViewWrapperCleaner());
    }

    public static void addLifecycleDispatcher(ILifecycleDispatcher dispatcher) {
        addLifecycleDispatcher(dispatcher, MugenService.IsNativeConfiguration);
    }

    public static void addLifecycleDispatcher(ILifecycleDispatcher dispatcher, boolean wrap) {
        if (wrap)
            dispatcher = new NativeLifecycleDispatcherWrapper(dispatcher);
        MugenService.addLifecycleDispatcher(dispatcher, 0);
    }

    public static void removeLifecycleDispatcher(ILifecycleDispatcher dispatcher) {
        MugenService.removeLifecycleDispatcher(dispatcher);
    }

    public static void addViewDispatcher(IViewDispatcher viewDispatcher) {
        MugenService.addViewDispatcher(viewDispatcher);
    }

    public static void removeViewDispatcher(IViewDispatcher viewDispatcher) {
        MugenService.removeViewDispatcher(viewDispatcher);
    }

    public static void setMainActivityMapping(int resource) {
        MugenExtensions.addViewMapping(MugenService.IsNativeConfiguration ? MainMugenAppCompatActivity.class : MainMugenActivity.Main.class, resource);
    }

    public static void addCommonActivityMapping(int resource) {
        MugenExtensions.addViewMapping(MugenService.IsNativeConfiguration ? MugenAppCompatActivity.class : MugenActivity.class, resource);
    }

    public static void addRecyclerViewMapping() {
        MugenExtensions.addWrapperMapping(RecyclerView.class, new IWrapperFactory() {
            @Override
            public Object wrap(Object view) {
                return new RecyclerViewWrapper(view);
            }

            @Override
            public int getPriority() {
                return 0;
            }
        });
    }

    public static void addToolbarCompatMapping() {
        MugenExtensions.addWrapperMapping(Toolbar.class, new IWrapperFactory() {
            @Override
            public Object wrap(Object view) {
                return new ToolbarCompatWrapper(view);
            }

            @Override
            public int getPriority() {
                return 0;
            }
        });
    }

    public static void addSwipeRefreshLayoutMapping() {
        MugenExtensions.addWrapperMapping(SwipeRefreshLayout.class, new IWrapperFactory() {
            @Override
            public Object wrap(Object view) {
                return new SwipeRefreshLayoutWrapper(view);
            }

            @Override
            public int getPriority() {
                return 0;
            }
        });
    }

    public static Object getView(Object container, int resourceId) {
        Object view = MugenService.getViewFactory().getView(container, resourceId);
        if (MugenService.IsNativeConfiguration)
            return MugenExtensions.wrap(view, true);
        return view;
    }

    public static boolean startActivity(IActivityView activityView, Class activityClass, int resourceId, int flags) {
        return MugenExtensions.startActivity(activityView, activityClass, resourceId, flags);
    }
}
