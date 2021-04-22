package com.mugen.mvvm.views;

import android.util.Log;
import android.view.Menu;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Checkable;
import android.widget.EditText;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;

import com.mugen.mvvm.MugenService;
import com.mugen.mvvm.MugenUtils;
import com.mugen.mvvm.constants.LifecycleState;
import com.mugen.mvvm.constants.MugenInitializationFlags;
import com.mugen.mvvm.interfaces.IContentItemsSourceProvider;
import com.mugen.mvvm.interfaces.IItemsSourceProviderBase;
import com.mugen.mvvm.interfaces.IMemberChangedListener;
import com.mugen.mvvm.interfaces.IMemberListener;
import com.mugen.mvvm.interfaces.IMemberListenerManager;
import com.mugen.mvvm.interfaces.IResourceItemsSourceProvider;
import com.mugen.mvvm.interfaces.views.IChildViewManager;
import com.mugen.mvvm.interfaces.views.IFragmentView;
import com.mugen.mvvm.interfaces.views.IHasLifecycleView;
import com.mugen.mvvm.interfaces.views.IViewContentManager;
import com.mugen.mvvm.interfaces.views.IViewItemsSourceManager;
import com.mugen.mvvm.interfaces.views.IViewMenuManager;
import com.mugen.mvvm.interfaces.views.IViewSelectedIndexManager;
import com.mugen.mvvm.internal.AttachedValues;
import com.mugen.mvvm.internal.MemberChangedListenerWrapper;
import com.mugen.mvvm.internal.ViewAttachedValues;
import com.mugen.mvvm.views.support.RecyclerViewMugenExtensions;
import com.mugen.mvvm.views.support.TabLayoutMugenExtensions;
import com.mugen.mvvm.views.support.TabLayoutTabMugenExtensions;
import com.mugen.mvvm.views.support.ViewPager2MugenExtensions;
import com.mugen.mvvm.views.support.ViewPagerMugenExtensions;

import java.util.ArrayList;
import java.util.HashSet;
import java.util.Objects;

public final class BindableMemberMugenExtensions {
    public static final CharSequence ParentMemberName = "Parent";
    public static final CharSequence ParentEventName = "ParentChanged";
    public static final CharSequence ClickEventName = "Click";
    public static final CharSequence LongClickEventName = "LongClick";
    public static final CharSequence TextMemberName = "Text";
    public static final CharSequence TextEventName = "TextChanged";
    public static final CharSequence HomeButtonClick = "HomeButtonClick";
    public static final CharSequence RefreshedEventName = "Refreshed";
    public final static CharSequence SelectedIndexName = "SelectedIndex";
    public final static CharSequence SelectedIndexEventName = "SelectedIndexChanged";
    public static final CharSequence CheckedMemberName = "Checked";
    public static final CharSequence CheckedEventName = "CheckedChanged";

    public static final int NoneProviderType = 0;
    public static final int ResourceProviderType = 1;
    public static final int ContentProviderType = 2;
    public static final int ContentRawProviderType = 3;
    public static final int ResourceOrContentProviderType = 4;
    protected final static Object NullParent = "";
    private static final HashSet<CharSequence> GlobalMemberListenerNames = new HashSet<CharSequence>() {{
        add(ParentEventName);
        add(ParentMemberName);
        add(HomeButtonClick);
    }};

    private BindableMemberMugenExtensions() {
    }

    public static HashSet<CharSequence> getGlobalMemberListenerNames() {
        return GlobalMemberListenerNames;
    }

    @Nullable
    public static IMemberChangedListener getMemberChangedListener(@NonNull Object target) {
        AttachedValues attachedValues = ViewMugenExtensions.getNativeAttachedValues(target, false);
        if (attachedValues == null)
            return null;
        return attachedValues.getMemberListener();
    }

    public static void setMemberChangedListener(@NonNull Object target, @NonNull IMemberChangedListener listener) {
        ViewMugenExtensions.getNativeAttachedValues(target, true).setMemberListener(listener);
    }

    public static boolean addMemberListener(@NonNull Object target, @NonNull CharSequence memberNameChar) {
        String memberName = (String) memberNameChar;
        if (GlobalMemberListenerNames.contains(memberNameChar))
            return true;

        AttachedValues attachedValues = ViewMugenExtensions.getNativeAttachedValues(target, false);
        MemberChangedListenerWrapper listeners = attachedValues == null ? null : attachedValues.getMemberListenerWrapper(false);
        if (listeners != null) {
            IMemberListener memberListener = listeners.get(memberName);
            if (memberListener != null) {
                memberListener.addListener(target, memberName);
                return true;
            }
        }

        ArrayList<IMemberListenerManager> memberListenerManagers = MugenService.getMemberListenerManagers();
        for (int i = 0; i < memberListenerManagers.size(); i++) {
            IMemberListener memberListener = memberListenerManagers.get(i).tryGetListener(target, memberName, listeners);
            if (memberListener != null) {
                if (listeners == null) {
                    if (attachedValues == null)
                        attachedValues = ViewMugenExtensions.getNativeAttachedValues(target, true);
                    listeners = attachedValues.getMemberListenerWrapper(true);
                }

                listeners.put(memberName, memberListener);
                memberListener.addListener(target, memberName);
                return true;
            }
        }
        return false;
    }

    public static boolean removeMemberListener(@NonNull Object target, @NonNull CharSequence memberName) {
        AttachedValues attachedValues = ViewMugenExtensions.getNativeAttachedValues(target, false);
        if (attachedValues == null)
            return false;

        MemberChangedListenerWrapper listeners = attachedValues.getMemberListenerWrapper(false);
        if (listeners == null)
            return false;

        IMemberListener memberListener = listeners.get((String) memberName);
        if (memberListener == null)
            return false;

        memberListener.removeListener(target, (String) memberName);
        return true;
    }

    public static boolean onMemberChanged(@NonNull Object target, @NonNull CharSequence memberName, @Nullable Object args) {
        AttachedValues attachedValues = ViewMugenExtensions.getNativeAttachedValues(target, false);
        if (attachedValues == null)
            return false;
        MemberChangedListenerWrapper listener = attachedValues.getMemberListenerWrapper(false);
        if (listener == null)
            return false;

        listener.onChanged(target, memberName, args);
        return true;
    }

    @Nullable
    public static Object getParent(@NonNull View view) {
        return getParentRaw(view);
    }

    public static void setParent(@NonNull View view, @Nullable Object parent) {
        if (view == parent) {
            if (MugenUtils.hasFlag(MugenInitializationFlags.Debug))
                Log.e(MugenUtils.LogTag, "setParent to self " + view);
            return;
        }

        Object oldParent = getParentRaw(view);
        parent = ViewMugenExtensions.tryWrap(parent);
        if (oldParent == parent)
            return;

        ViewMugenExtensions.getNativeAttachedValues(view, true).setParent(parent == null ? NullParent : parent);
        onMemberChanged(view, ParentMemberName, null);
    }

    public static boolean isChildRecycleSupported(@NonNull View view) {
        IChildViewManager childViewManager = MugenService.getChildViewManager();
        if (childViewManager != null && childViewManager.isSupported(view))
            return childViewManager.isChildRecycleSupported(view);
        return !TabLayoutMugenExtensions.isSupported(view);
    }

    public static boolean getChecked(@NonNull View view) {
        return ((Checkable) view).isChecked();
    }

    public static void setChecked(@NonNull View view, boolean value) {
        ((Checkable) view).setChecked(value);
    }

    @Nullable
    public static CharSequence getText(@NonNull View view) {
        return ((TextView) view).getText();
    }

    public static void setText(@NonNull View view, @Nullable CharSequence text) {
        TextView txtView = (TextView) view;
        CharSequence txt = txtView.getText();
        if (text != null && text.equals(txt) || (text instanceof String && ((String) text).contentEquals(txt)))
            return;

        txtView.setText(text);
        if (text != null && view instanceof EditText)
            ((EditText) view).setSelection(((EditText) view).length());
    }

    @Nullable
    public static CharSequence getText(@NonNull Object view) {
        if (TabLayoutTabMugenExtensions.isSupported(view))
            return TabLayoutTabMugenExtensions.getText(view);
        return getText((View) view);
    }

    public static void setText(@NonNull Object view, @Nullable CharSequence text) {
        if (TabLayoutTabMugenExtensions.isSupported(view))
            TabLayoutTabMugenExtensions.getText(view);
        else
            setText((View) view, text);
    }

    @Nullable
    public static Object getChildAt(@NonNull View view, int index) {
        IChildViewManager childViewManager = MugenService.getChildViewManager();
        if (childViewManager != null && childViewManager.isSupported(view))
            return childViewManager.getChildAt(view, index);

        if (TabLayoutMugenExtensions.isSupported(view))
            return TabLayoutMugenExtensions.getTabAt(view, index);
        return ((ViewGroup) view).getChildAt(index);
    }

    public static void addChild(@NonNull View view, @NonNull Object child, int position, boolean setSelected) {
        IChildViewManager childViewManager = MugenService.getChildViewManager();
        if (childViewManager != null && childViewManager.isSupported(view))
            childViewManager.addChild(view, child, position, setSelected);
        else if (TabLayoutMugenExtensions.isSupported(view))
            TabLayoutMugenExtensions.addTab(view, child, position, setSelected);
        else
            ((ViewGroup) view).addView((View) child, position);
    }

    public static void removeChildAt(@NonNull View view, int position) {
        IChildViewManager childViewManager = MugenService.getChildViewManager();
        if (childViewManager != null && childViewManager.isSupported(view))
            childViewManager.removeChildAt(view, position);
        else if (TabLayoutMugenExtensions.isSupported(view))
            TabLayoutMugenExtensions.removeTab(view, position);
        else
            ((ViewGroup) view).removeViewAt(position);
    }

    public static void clear(@NonNull View view) {
        IChildViewManager childViewManager = MugenService.getChildViewManager();
        if (childViewManager != null && childViewManager.isSupported(view))
            childViewManager.clear(view);
        else if (TabLayoutMugenExtensions.isSupported(view))
            TabLayoutMugenExtensions.clearTabs(view);
        else
            ((ViewGroup) view).removeAllViews();
    }

    public static boolean isMenuSupported(@NonNull Object view) {
        IViewMenuManager menuManager = MugenService.getMenuManager();
        return menuManager != null && menuManager.isMenuSupported(view) || ToolbarMugenExtensions.isSupported(view);
    }

    @NonNull
    public static Menu getMenu(@NonNull Object view) {
        IViewMenuManager menuManager = MugenService.getMenuManager();
        if (menuManager != null && menuManager.isMenuSupported(view))
            return menuManager.getMenu(view);
        if (MaterialComponentMugenExtensions.isMenuSupported(view))
            return MaterialComponentMugenExtensions.getMenu(view);
        return ToolbarMugenExtensions.getMenu((View) view);
    }

    public static boolean isSelectedIndexSupported(@NonNull View view) {
        IViewSelectedIndexManager manager = MugenService.getSelectedIndexManager();
        if (manager != null && manager.isSelectedIndexSupported(view))
            return true;
        return ViewPagerMugenExtensions.isSupported(view) || ViewPager2MugenExtensions.isSupported(view) || TabLayoutMugenExtensions.isSupported(view);
    }

    public static int getSelectedIndex(@NonNull View view) {
        IViewSelectedIndexManager manager = MugenService.getSelectedIndexManager();
        if (manager != null && manager.isSelectedIndexSupported(view))
            return manager.getSelectedIndex(view);
        if (ViewPagerMugenExtensions.isSupported(view))
            return ViewPagerMugenExtensions.getCurrentItem(view);
        if (ViewPager2MugenExtensions.isSupported(view))
            return ViewPager2MugenExtensions.getCurrentItem(view);
        if (TabLayoutMugenExtensions.isSupported(view))
            return TabLayoutMugenExtensions.getSelectedTabPosition(view);
        return -1;
    }

    public static boolean setSelectedIndex(@NonNull View view, int index, boolean smoothScroll) {
        IViewSelectedIndexManager manager = MugenService.getSelectedIndexManager();
        if (manager != null && manager.isSelectedIndexSupported(view)) {
            manager.setSelectedIndex(view, index, smoothScroll);
            return true;
        }
        if (ViewPagerMugenExtensions.isSupported(view)) {
            ViewPagerMugenExtensions.setCurrentItem(view, index, smoothScroll);
            return true;
        }
        if (ViewPager2MugenExtensions.isSupported(view)) {
            ViewPager2MugenExtensions.setCurrentItem(view, index, smoothScroll);
            return true;
        }
        if (TabLayoutMugenExtensions.isSupported(view)) {
            TabLayoutMugenExtensions.setSelectedTabPosition(view, index);
            return true;
        }
        return false;
    }

    public static int getItemSourceProviderType(@NonNull View view) {
        IViewItemsSourceManager manager = MugenService.getItemsSourceManager();
        if (manager != null && manager.isItemsSourceSupported(view))
            return manager.getItemSourceProviderType(view);
        if (RecyclerViewMugenExtensions.isSupported(view))
            return RecyclerViewMugenExtensions.ItemsSourceProviderType;
        if (ViewPager2MugenExtensions.isSupported(view))
            return ViewPager2MugenExtensions.ItemsSourceProviderType;
        if (ViewPagerMugenExtensions.isSupported(view))
            return ViewPagerMugenExtensions.ItemsSourceProviderType;
        if (AdapterViewMugenExtensions.isSupported(view))
            return AdapterViewMugenExtensions.ItemsSourceProviderType;
        if (TabLayoutMugenExtensions.isSupported(view))
            return TabLayoutMugenExtensions.ItemsSourceProviderType;
        if (view instanceof ViewGroup)
            return ContentRawProviderType;
        return NoneProviderType;
    }

    @Nullable
    public static IItemsSourceProviderBase getItemsSourceProvider(@NonNull View view) {
        IViewItemsSourceManager manager = MugenService.getItemsSourceManager();
        if (manager != null && manager.isItemsSourceSupported(view))
            return manager.getItemsSourceProvider(view);
        if (RecyclerViewMugenExtensions.isSupported(view))
            return RecyclerViewMugenExtensions.getItemsSourceProvider(view);
        if (ViewPager2MugenExtensions.isSupported(view))
            return ViewPager2MugenExtensions.getItemsSourceProvider(view);
        if (ViewPagerMugenExtensions.isSupported(view))
            return ViewPagerMugenExtensions.getItemsSourceProvider(view);
        if (AdapterViewMugenExtensions.isSupported(view))
            return AdapterViewMugenExtensions.getItemsSourceProvider(view);
        return null;
    }

    public static void setItemsSourceProvider(@NonNull View view, @Nullable IItemsSourceProviderBase provider, boolean hasFragments) {
        IViewItemsSourceManager manager = MugenService.getItemsSourceManager();
        if (manager != null && manager.isItemsSourceSupported(view))
            manager.setItemsSourceProvider(view, provider, hasFragments);
        else if (RecyclerViewMugenExtensions.isSupported(view))
            RecyclerViewMugenExtensions.setItemsSourceProvider(view, (IResourceItemsSourceProvider) provider);
        else if (ViewPager2MugenExtensions.isSupported(view))
            ViewPager2MugenExtensions.setItemsSourceProvider(view, provider, hasFragments);
        else if (ViewPagerMugenExtensions.isSupported(view))
            ViewPagerMugenExtensions.setItemsSourceProvider(view, (IContentItemsSourceProvider) provider, hasFragments);
        else if (AdapterViewMugenExtensions.isSupported(view))
            AdapterViewMugenExtensions.setItemsSourceProvider(view, (IResourceItemsSourceProvider) provider);
    }

    @Nullable
    public static Object getContent(@NonNull View view) {
        IViewContentManager contentManager = MugenService.getContentManager();
        if (contentManager != null && contentManager.isContentSupported(view))
            return contentManager.getContent(view);

        ViewGroup viewGroup = (ViewGroup) view;
        if (viewGroup.getChildCount() == 0)
            return null;
        View result = viewGroup.getChildAt(0);
        if (result == null)
            return null;
        ViewAttachedValues attachedValues = ViewMugenExtensions.getNativeAttachedValues(result, false);
        if (attachedValues == null)
            return result;
        Fragment fragment = (Fragment) attachedValues.getFragment();
        if (fragment == null)
            return result;
        return fragment;
    }

    public static void setContent(@NonNull View view, @Nullable Object content) {
        IViewContentManager contentManager = MugenService.getContentManager();
        if (contentManager != null && contentManager.isContentSupported(view)) {
            contentManager.setContent(view, content);
            return;
        }

        Object oldContent = getContent(view);
        if (Objects.equals(oldContent, content))
            return;

        ViewGroup viewGroup = (ViewGroup) view;
        if (MugenUtils.isCompatSupported()) {
            if (content == null && FragmentMugenExtensions.setFragment(view, null))
                return;
            if (FragmentMugenExtensions.isSupported(content) && FragmentMugenExtensions.setFragment(view, (IFragmentView) content))
                return;
        }

        boolean hasLifecycleOld = oldContent != null && !(oldContent instanceof IHasLifecycleView);
        boolean hasLifecycleNew = content != null && !(content instanceof IHasLifecycleView);
        if (hasLifecycleOld)
            LifecycleMugenExtensions.onLifecycleChanging(oldContent, LifecycleState.Pause, null);
        if (hasLifecycleNew)
            LifecycleMugenExtensions.onLifecycleChanging(content, LifecycleState.Resume, null);

        viewGroup.removeAllViews();
        if (content != null)
            viewGroup.addView((View) content);

        if (hasLifecycleOld)
            LifecycleMugenExtensions.onLifecycleChanged(oldContent, LifecycleState.Pause, null);
        if (hasLifecycleNew)
            LifecycleMugenExtensions.onLifecycleChanged(content, LifecycleState.Resume, null);
    }

    @Nullable
    public static Object findRelativeSource(@NonNull View view, @NonNull String name, int level) {
        int nameLevel = 0;
        Object target = getParentRaw(view);
        while (target != null) {
            if (typeNameEqual(target.getClass(), name) && ++nameLevel == level)
                return target;

            if (target instanceof View)
                target = getParentRaw((View) target);
            else
                target = null;
        }

        return null;
    }

    @Nullable
    private static Object getParentRaw(@NonNull View view) {
        if (view.getId() == android.R.id.content)
            return ViewMugenExtensions.tryWrap(ActivityMugenExtensions.tryGetActivity(view.getContext()));

        ViewAttachedValues attachedValues = ViewMugenExtensions.getNativeAttachedValues(view, false);
        Object parent = attachedValues == null ? null : attachedValues.getParent();
        if (parent == NullParent)
            return null;

        if (parent == null)
            parent = view.getParent();
        return parent;
    }

    private static boolean typeNameEqual(@Nullable Class clazz, @NonNull String typeName) {
        while (clazz != null) {
            if (clazz.getSimpleName().equals(typeName))
                return true;
            clazz = clazz.getSuperclass();
        }
        return false;
    }
}
