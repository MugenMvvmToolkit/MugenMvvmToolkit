package com.mugen.mvvm.views.support;

import android.view.View;
import androidx.viewpager.widget.PagerAdapter;
import androidx.viewpager.widget.ViewPager;
import com.mugen.mvvm.interfaces.IContentItemsSourceProvider;
import com.mugen.mvvm.interfaces.IItemsSourceProviderBase;
import com.mugen.mvvm.interfaces.views.IListView;
import com.mugen.mvvm.internal.NativeContentItemsSourceProviderWrapper;
import com.mugen.mvvm.internal.ViewParentObserver;
import com.mugen.mvvm.internal.support.MugenPagerAdapter;
import com.mugen.mvvm.views.ViewWrapper;

public class ViewPagerWrapper extends ViewWrapper implements IListView {
    public ViewPagerWrapper(Object view) {
        super(view);
        ViewParentObserver.Instance.remove((View) view, true);
    }

    @Override
    public IItemsSourceProviderBase getItemsSourceProvider() {
        ViewPager view = (ViewPager) getView();
        if (view == null)
            return null;
        return getProvider(view);
    }

    @Override
    public void setItemsSourceProvider(IItemsSourceProviderBase provider) {
        ViewPager view = (ViewPager) getView();
        if (view != null)
            setItemsSourceProvider(view, (IContentItemsSourceProvider) provider);
    }

    @Override
    protected void onReleased(View target) {
        super.onReleased(target);
        setItemsSourceProvider((ViewPager) target, null);
    }

    private IContentItemsSourceProvider getProvider(ViewPager view) {
        PagerAdapter adapter = view.getAdapter();
        if (adapter instanceof MugenPagerAdapter) {
            IContentItemsSourceProvider provider = ((MugenPagerAdapter) adapter).getItemsSourceProvider();
            if (provider != null)
                return ((NativeContentItemsSourceProviderWrapper) provider).getNestedProvider();
        }
        return null;
    }

    private void setItemsSourceProvider(ViewPager view, IContentItemsSourceProvider provider) {
        if (getProvider(view) == provider)
            return;
        PagerAdapter adapter = view.getAdapter();
        if (provider == null) {
            if (adapter instanceof MugenPagerAdapter)
                ((MugenPagerAdapter) adapter).detach();
            view.setAdapter(null);
        } else
            view.setAdapter(new MugenPagerAdapter(view, new NativeContentItemsSourceProviderWrapper(view, provider)));
    }
}
