package com.mugen.mvvm.views;

import android.view.View;
import com.mugen.mvvm.R;
import com.mugen.mvvm.extensions.MugenExtensions;
import com.mugen.mvvm.models.WeakTargetBase;
import com.mugen.mvvm.interfaces.IMemberObserver;
import com.mugen.mvvm.interfaces.views.IAndroidView;
import com.mugen.mvvm.interfaces.views.IResourceView;

public class ViewWrapper extends WeakTargetBase<View> implements IAndroidView, View.OnClickListener {
    public static final Object NullParent = "";

    private short _clickListenerCount;
    private IMemberObserver _memberObserver;

    //todo using Object as type to prevent xamarin linker keep View
    public ViewWrapper(Object view) {
        super((View) view);
    }

    @Override
    public int getVisibility() {
        View view = getView();
        if (view == null)
            return 0;
        return view.getVisibility();
    }

    @Override
    public void setVisibility(int visibility) {
        View view = getView();
        if (view != null)
            view.setVisibility(visibility);
    }

    @Override
    public void setBackgroundColor(int color) {
        View view = getView();
        if (view != null)
            view.setBackgroundColor(color);
    }

    @Override
    public final void addMemberListener(String memberName) {
        View view = getView();
        if (view != null)
            addMemberListener(view, memberName);
    }

    @Override
    public final void removeMemberListener(String memberName) {
        View view = getView();
        if (view != null)
            removeMemberListener(view, memberName);
    }

    @Override
    public final void onMemberChanged(String member, Object args) {
        IMemberObserver memberObserver = getMemberObserver();
        if (memberObserver != null)
            onMemberChanged(memberObserver, member, args);
    }

    @Override
    public View getView() {
        return getTarget();
    }

    @Override
    public Object getParent() {
        View view = getView();
        if (view == null)
            return null;
        return MugenExtensions.wrap(getParentRaw(view), true);
    }

    @Override
    public Object findRelativeSource(String name, int level) {
        View view = getView();
        if (view == null)
            return null;
        int nameLevel = 0;
        Object target = getParentRaw(view);
        while (target != null) {
            if (typeNameEqual(target.getClass(), name) && ++nameLevel == level)
                return MugenExtensions.wrap(target, true);

            if (target instanceof View)
                target = getParentRaw((View) target);
            else
                target = null;
        }

        return null;
    }

    @Override
    public Object getElementById(int id) {
        View view = getView();
        if (view == null)
            return null;
        return MugenExtensions.wrap(view.findViewById(id), true);
    }

    @Override
    public void setParent(Object parent) {
        View view = getView();
        if (view == null)
            return;
        view.setTag(R.id.parent, MugenExtensions.wrap(parent, true));
        onMemberChanged(ParentMemberName, null);
    }

    @Override
    public IMemberObserver getMemberObserver() {
        return _memberObserver;
    }

    @Override
    public void setMemberObserver(IMemberObserver listener) {
        _memberObserver = listener;
    }

    @Override
    public Object getTag(int id) {
        View view = getView();
        if (view == null)
            return null;
        return view.getTag(id);
    }

    @Override
    public void setTag(int id, Object state) {
        View view = getView();
        if (view != null)
            view.setTag(id, state);
    }

    @Override
    public int getViewId() {
        View view = getView();
        if (view instanceof IResourceView)
            return ((IResourceView) view).getViewId();
        return 0;
    }

    @Override
    public void onClick(View v) {
        onMemberChanged(ClickEventName, null);
    }

    protected void onMemberChanged(IMemberObserver observer, String member, Object args) {
        observer.onMemberChanged(this, member, args);
        if (ParentMemberName.equals(member))
            observer.onMemberChanged(this, ParentEventName, args);
    }

    protected void addMemberListener(View view, String memberName) {
        if (ClickEventName.equals(memberName) && _clickListenerCount++ == 0)
            view.setOnClickListener(this);
    }

    protected void removeMemberListener(View view, String memberName) {
        if (ClickEventName.equals(memberName) && _clickListenerCount != 0 && --_clickListenerCount == 0) {
            view.setOnClickListener(null);
        }
    }

    private static boolean typeNameEqual(Class clazz, String typeName) {
        while (clazz != null) {
            if (clazz.getSimpleName().equals(typeName))
                return true;
            clazz = clazz.getSuperclass();
        }
        return false;
    }

    private static Object getParentRaw(View view) {
        if (view.getId() == android.R.id.content)
            return MugenExtensions.getActivity(view.getContext());

        Object parent = view.getTag(R.id.parent);
        if (parent == NullParent)
            return null;

        if (parent == null)
            parent = view.getParent();
        return parent;
    }
}
