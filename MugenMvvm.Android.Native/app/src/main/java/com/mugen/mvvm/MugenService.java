package com.mugen.mvvm;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.mugen.mvvm.interfaces.IActivityManager;
import com.mugen.mvvm.interfaces.IAsyncAppInitializer;
import com.mugen.mvvm.interfaces.IAttachedValueProvider;
import com.mugen.mvvm.interfaces.ILifecycleDispatcher;
import com.mugen.mvvm.interfaces.IMemberListenerManager;
import com.mugen.mvvm.interfaces.IWrapperManager;
import com.mugen.mvvm.interfaces.views.IChildViewManager;
import com.mugen.mvvm.interfaces.views.IViewContentManager;
import com.mugen.mvvm.interfaces.views.IViewDispatcher;
import com.mugen.mvvm.interfaces.views.IViewFactory;
import com.mugen.mvvm.interfaces.views.IViewItemsSourceManager;
import com.mugen.mvvm.interfaces.views.IViewMenuManager;
import com.mugen.mvvm.interfaces.views.IViewSelectedIndexManager;
import com.mugen.mvvm.internal.HasPriorityComparator;
import com.mugen.mvvm.internal.NativeLifecycleDispatcherWrapper;
import com.mugen.mvvm.internal.ViewFactory;

import java.util.ArrayList;
import java.util.Collections;

public final class MugenService {
    private final static ArrayList<IViewDispatcher> ViewDispatchers = new ArrayList<>();
    private final static ArrayList<IMemberListenerManager> ListenerManagers = new ArrayList<>();
    private final static ArrayList<ILifecycleDispatcher> LifecycleDispatchers = new ArrayList<>();
    private static IViewFactory _viewFactory;
    private static IAttachedValueProvider _attachedValueProvider;
    private static IWrapperManager _wrapperManager;
    private static IViewMenuManager _menuManager;
    private static IViewSelectedIndexManager _selectedIndexManager;
    private static IViewContentManager _contentManager;
    private static IViewItemsSourceManager _itemsSourceManager;
    private static IActivityManager _activityManager;
    private static IChildViewManager _childViewManager;
    private static IAsyncAppInitializer _asyncAppInitializer;

    private MugenService() {
    }

    public static IAsyncAppInitializer getAsyncAppInitializer() {
        return _asyncAppInitializer;
    }

    public static void setAsyncAppInitializer(IAsyncAppInitializer initializer) {
        _asyncAppInitializer = initializer;
    }

    public static ArrayList<IMemberListenerManager> getMemberListenerManagers() {
        return ListenerManagers;
    }

    public static void addMemberListenerManager(@NonNull IMemberListenerManager manager) {
        ListenerManagers.add(manager);
        Collections.sort(ListenerManagers, HasPriorityComparator.Instance);
    }

    public static boolean removeMemberListenerManager(@NonNull IMemberListenerManager manager) {
        return ListenerManagers.remove(manager);
    }

    public static ArrayList<IViewDispatcher> getViewDispatchers() {
        return ViewDispatchers;
    }

    public static void addViewDispatcher(@NonNull IViewDispatcher viewDispatcher) {
        ViewDispatchers.add(viewDispatcher);
        Collections.sort(ViewDispatchers, HasPriorityComparator.Instance);
    }

    public static void removeViewDispatcher(@NonNull IViewDispatcher viewDispatcher) {
        ViewDispatchers.remove(viewDispatcher);
    }

    public static ArrayList<ILifecycleDispatcher> getLifecycleDispatchers() {
        return LifecycleDispatchers;
    }

    public static void addLifecycleDispatcher(@NonNull ILifecycleDispatcher dispatcher, boolean wrap) {
        if (wrap)
            dispatcher = new NativeLifecycleDispatcherWrapper(dispatcher);
        LifecycleDispatchers.add(dispatcher);
        Collections.sort(LifecycleDispatchers, HasPriorityComparator.Instance);
    }

    public static void removeLifecycleDispatcher(@NonNull ILifecycleDispatcher dispatcher) {
        if (LifecycleDispatchers.remove(dispatcher))
            return;
        for (int i = 0; i < LifecycleDispatchers.size(); i++) {
            ILifecycleDispatcher d = LifecycleDispatchers.get(i);
            if (d instanceof NativeLifecycleDispatcherWrapper && ((NativeLifecycleDispatcherWrapper) d).getNestedDispatcher().equals(dispatcher)) {
                LifecycleDispatchers.remove(i);
                return;
            }
        }
    }

    @NonNull
    public static IViewFactory getViewFactory() {
        if (_viewFactory == null)
            setViewFactory(new ViewFactory());
        return _viewFactory;
    }

    public static void setViewFactory(@Nullable IViewFactory viewFactory) {
        if (_viewFactory instanceof ILifecycleDispatcher)
            removeLifecycleDispatcher((ILifecycleDispatcher) _viewFactory);
        _viewFactory = viewFactory;
        if (_viewFactory instanceof ILifecycleDispatcher)
            addLifecycleDispatcher((ILifecycleDispatcher) _viewFactory, false);
    }

    @Nullable
    public static IActivityManager getActivityManager() {
        return _activityManager;
    }

    public static void setActivityManager(@Nullable IActivityManager activityManager) {
        _activityManager = activityManager;
    }

    @Nullable
    public static IChildViewManager getChildViewManager() {
        return _childViewManager;
    }

    public static void setChildViewManager(@Nullable IChildViewManager childViewManager) {
        _childViewManager = childViewManager;
    }

    @Nullable
    public static IViewContentManager getContentManager() {
        return _contentManager;
    }

    public static void setContentManager(@Nullable IViewContentManager contentManager) {
        _contentManager = contentManager;
    }

    @Nullable
    public static IViewItemsSourceManager getItemsSourceManager() {
        return _itemsSourceManager;
    }

    public static void setItemsSourceManager(@Nullable IViewItemsSourceManager itemsSourceManager) {
        _itemsSourceManager = itemsSourceManager;
    }

    @Nullable
    public static IViewSelectedIndexManager getSelectedIndexManager() {
        return _selectedIndexManager;
    }

    public static void setSelectedIndexManager(@Nullable IViewSelectedIndexManager selectedIndexManager) {
        _selectedIndexManager = selectedIndexManager;
    }

    @Nullable
    public static IAttachedValueProvider getAttachedValueProvider() {
        return _attachedValueProvider;
    }

    public static void setAttachedValueProvider(@Nullable IAttachedValueProvider provider) {
        _attachedValueProvider = provider;
    }

    @Nullable
    public static IViewMenuManager getMenuManager() {
        return _menuManager;
    }

    public static void setMenuManager(@Nullable IViewMenuManager menuManager) {
        _menuManager = menuManager;
    }

    @Nullable
    public static IWrapperManager getWrapperManager() {
        return _wrapperManager;
    }

    public static void setWrapperManager(@Nullable IWrapperManager wrapperManager) {
        _wrapperManager = wrapperManager;
    }
}
