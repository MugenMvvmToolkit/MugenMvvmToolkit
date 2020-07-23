package com.mugen.mvvm.views.support;

import android.view.View;
import androidx.recyclerview.widget.RecyclerView;
import androidx.viewpager2.widget.ViewPager2;
import com.mugen.mvvm.interfaces.IItemsSourceProviderBase;
import com.mugen.mvvm.interfaces.IResourceItemsSourceProvider;
import com.mugen.mvvm.interfaces.views.IViewPager;
import com.mugen.mvvm.internal.NativeResourceItemsSourceProviderWrapper;
import com.mugen.mvvm.internal.ViewParentObserver;
import com.mugen.mvvm.internal.support.MugenRecyclerViewAdapter;
import com.mugen.mvvm.views.ViewWrapper;

public class ViewPager2Wrapper extends ViewWrapper implements IViewPager {
    private short _selectedIndexChangedCount;
    private Listener _listener;

    public ViewPager2Wrapper(Object view) {
        super(view);
        ViewParentObserver.Instance.remove((View) view, true);
    }

    @Override
    public int getProviderType() {
        return ItemSourceProviderType;
    }

    @Override
    public IItemsSourceProviderBase getItemsSourceProvider() {
        ViewPager2 view = (ViewPager2) getView();
        if (view == null)
            return null;
        return getProvider(view);
    }

    @Override
    public void setItemsSourceProvider(IItemsSourceProviderBase provider) {
        ViewPager2 view = (ViewPager2) getView();
        if (view == null || getProvider(view) == provider)
            return;
        if (provider == null) {
            view.setAdapter(null);
        } else
            view.setAdapter(new MugenRecyclerViewAdapter(view.getContext(), new NativeResourceItemsSourceProviderWrapper((IResourceItemsSourceProvider) provider)));
    }

    @Override
    public int getSelectedIndex() {
        ViewPager2 view = (ViewPager2) getView();
        if (view == null)
            return 0;
        return view.getCurrentItem();
    }

    @Override
    public void setSelectedIndex(int index) {
        setSelectedIndex(index, true);
    }

    @Override
    public void setSelectedIndex(int index, boolean smoothScroll) {
        ViewPager2 view = (ViewPager2) getView();
        if (view != null)
            view.setCurrentItem(index, smoothScroll);
    }

    @Override
    protected void addMemberListener(View view, String memberName) {
        super.addMemberListener(view, memberName);
        if (SelectedIndexName.equals(memberName) || SelectedIndexEventName.equals(memberName) && _selectedIndexChangedCount++ == 0) {
            if (_listener == null)
                _listener = new Listener();
            ((ViewPager2) view).registerOnPageChangeCallback(_listener);
        }
    }

    @Override
    protected void removeMemberListener(View view, String memberName) {
        super.removeMemberListener(view, memberName);
        if (SelectedIndexName.equals(memberName) || SelectedIndexEventName.equals(memberName) && _selectedIndexChangedCount != 0 && --_selectedIndexChangedCount == 0)
            ((ViewPager2) view).unregisterOnPageChangeCallback(_listener);
    }

    @Override
    protected void onReleased(View target) {
        super.onReleased(target);
        ((ViewPager2) target).setAdapter(null);
    }

    private IResourceItemsSourceProvider getProvider(ViewPager2 view) {
        RecyclerView.Adapter adapter = view.getAdapter();
        if (adapter instanceof MugenRecyclerViewAdapter) {
            IResourceItemsSourceProvider provider = ((MugenRecyclerViewAdapter) adapter).getItemsSourceProvider();
            if (provider != null)
                return ((NativeResourceItemsSourceProviderWrapper) provider).getNestedProvider();
        }
        return null;
    }

    private final class Listener extends ViewPager2.OnPageChangeCallback {
        @Override
        public void onPageSelected(int position) {
            onMemberChanged(SelectedIndexName, null);
            onMemberChanged(SelectedIndexEventName, null);
        }
    }
}
