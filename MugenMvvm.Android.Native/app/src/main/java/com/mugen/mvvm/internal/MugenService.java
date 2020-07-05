package com.mugen.mvvm.internal;

import android.content.Context;
import android.util.AttributeSet;
import android.view.View;
import android.widget.AdapterView;
import android.widget.TextView;
import androidx.recyclerview.widget.RecyclerView;
import androidx.swiperefreshlayout.widget.SwipeRefreshLayout;
import com.mugen.mvvm.extensions.MugenExtensions;
import com.mugen.mvvm.interfaces.ILifecycleDispatcher;
import com.mugen.mvvm.interfaces.views.IViewDispatcher;
import com.mugen.mvvm.interfaces.views.IWrapperFactory;
import com.mugen.mvvm.views.AdapterViewWrapper;
import com.mugen.mvvm.views.TextViewWrapper;
import com.mugen.mvvm.views.support.RecyclerViewWrapper;
import com.mugen.mvvm.views.support.SwipeRefreshLayoutWrapper;

import java.util.ArrayList;

public final class MugenService {
    public static boolean IsNativeConfiguration;
    public static boolean IncludeSupportLibs;
    private static Context _context;
    private final static ArrayList<ILifecycleDispatcher> _lifecycleDispatchers = new ArrayList<>();
    private final static ArrayList<IViewDispatcher> _viewDispatchers = new ArrayList<>();

    private MugenService() {
    }

    public static void initialize(Context context) {
        _context = context;
    }

    public static void initializeNative(boolean includeSupportLibs) {
        IsNativeConfiguration = true;
        IncludeSupportLibs = includeSupportLibs;
        MugenExtensions.addWrapperMapping(TextView.class, new IWrapperFactory() {
            @Override
            public Object wrap(Object view) {
                return new TextViewWrapper((TextView) view);
            }

            @Override
            public int getPriority() {
                return 0;
            }
        });
        MugenExtensions.addWrapperMapping(AdapterView.class, new IWrapperFactory() {
            @Override
            public Object wrap(Object view) {
                return new AdapterViewWrapper(view);
            }

            @Override
            public int getPriority() {
                return 0;
            }
        });
        if (includeSupportLibs) {
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
    }

    public static Context getAppContext() {
        return _context;
    }

    public static void addLifecycleDispatcher(ILifecycleDispatcher dispatcher) {
        _lifecycleDispatchers.add(dispatcher);
    }

    public static void removeLifecycleDispatcher(ILifecycleDispatcher dispatcher) {
        if (_lifecycleDispatchers.remove(dispatcher))
            return;
        for (int i = 0; i < _lifecycleDispatchers.size(); i++) {
            ILifecycleDispatcher d = _lifecycleDispatchers.get(i);
            if (d instanceof NativeLifecycleDispatcherWrapper && ((NativeLifecycleDispatcherWrapper) d).getNestedDispatcher().equals(dispatcher)) {
                _lifecycleDispatchers.remove(i);
                return;
            }
        }
    }

    public static void addViewDispatcher(IViewDispatcher viewDispatcher) {
        _viewDispatchers.add(viewDispatcher);
    }

    public static void removeViewDispatcher(IViewDispatcher viewDispatcher) {
        _viewDispatchers.remove(viewDispatcher);
    }

    public static boolean onLifecycleChanging(Object target, int lifecycle, Object state) {
        boolean result = true;
        for (int i = 0; i < _lifecycleDispatchers.size(); i++) {
            if (!_lifecycleDispatchers.get(i).onLifecycleChanging(target, lifecycle, state))
                result = false;
        }
        return result;
    }

    public static void onLifecycleChanged(Object target, int lifecycle, Object state) {
        for (int i = 0; i < _lifecycleDispatchers.size(); i++) {
            _lifecycleDispatchers.get(i).onLifecycleChanged(target, lifecycle, state);
        }
    }

    public static void onParentChanged(View view) {
        for (int i = 0; i < _viewDispatchers.size(); i++) {
            _viewDispatchers.get(i).onParentChanged(view);
        }
    }

    public static void onSetView(Object owner, View view) {
        for (int i = 0; i < _viewDispatchers.size(); i++) {
            _viewDispatchers.get(i).onSetView(owner, view);
        }
    }

    public static void onInflatingView(int resourceId, Context context) {
        for (int i = 0; i < _viewDispatchers.size(); i++) {
            _viewDispatchers.get(i).onInflatingView(resourceId, context);
        }
    }

    public static void onInflatedView(View view, int resourceId, Context context) {
        for (int i = 0; i < _viewDispatchers.size(); i++) {
            _viewDispatchers.get(i).onInflatedView(view, resourceId, context);
        }
    }

    public static View onViewCreated(View view, Context context, AttributeSet attrs) {
        if (view == null)
            return null;
        for (int i = 0; i < _viewDispatchers.size(); i++) {
            view = _viewDispatchers.get(i).onViewCreated(view, context, attrs);
        }
        return view;
    }

    public static View tryCreateCustomView(View parent, String name, Context viewContext, AttributeSet attrs) {
        for (int i = 0; i < _viewDispatchers.size(); i++) {
            View view = _viewDispatchers.get(i).tryCreateCustomView(parent, name, viewContext, attrs);
            if (view != null)
                return view;
        }
        return null;
    }
}
