package com.mugen.mvvm.internal.support;

import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.viewpager.widget.PagerAdapter;
import androidx.viewpager.widget.ViewPager;

import com.mugen.mvvm.constants.BindableMemberConstant;
import com.mugen.mvvm.constants.LifecycleState;
import com.mugen.mvvm.interfaces.IContentItemsSourceProvider;
import com.mugen.mvvm.interfaces.IItemsSourceObserver;
import com.mugen.mvvm.interfaces.IMugenAdapter;
import com.mugen.mvvm.views.BindableMemberMugenExtensions;
import com.mugen.mvvm.views.LifecycleMugenExtensions;

public class MugenPagerAdapter extends PagerAdapter implements IItemsSourceObserver, IMugenAdapter {
    protected final ViewPager _viewPager;
    protected final IContentItemsSourceProvider _provider;
    protected Object _currentPrimaryItem;

    public MugenPagerAdapter(@NonNull ViewPager viewPager, @NonNull IContentItemsSourceProvider provider) {
        _viewPager = viewPager;
        _provider = provider;
        provider.addObserver(this);
    }

    @NonNull
    public IContentItemsSourceProvider getItemsSourceProvider() {
        return _provider;
    }

    public void detach() {
        _provider.removeObserver(this);
    }

    @Override
    public int getCount() {
        return _provider.getCount();
    }

    @Nullable
    @Override
    public CharSequence getPageTitle(int position) {
        return _provider.getItemTitle(position);
    }

    @NonNull
    @Override
    public Object instantiateItem(@NonNull ViewGroup container, int position) {
        Object content = _provider.getContent(position);
        if (content == null) {
            TextView txt = new TextView(container.getContext());
            txt.setText("null");
            content = txt;
        }

        container.addView((View) content);
        return content;
    }

    @Override
    public void destroyItem(@NonNull ViewGroup container, int position, @NonNull Object object) {
        LifecycleMugenExtensions.onLifecycleChanging(object, LifecycleState.Destroy, null, false);
        container.removeView((View) object);
        LifecycleMugenExtensions.onLifecycleChanged(object, LifecycleState.Destroy, null);
    }

    @Override
    public int getItemPosition(@NonNull Object object) {
        return _provider.getContentPosition(object);
    }

    @Override
    public boolean isViewFromObject(@NonNull View view, @NonNull Object object) {
        return view == object;
    }

    @Override
    public void setPrimaryItem(@NonNull ViewGroup container, int position, @NonNull Object object) {
        Object oldContent = _currentPrimaryItem;
        if (oldContent != object) {
            if (oldContent != null) {
                LifecycleMugenExtensions.onLifecycleChanging(oldContent, LifecycleState.Disappear, null, false);
                LifecycleMugenExtensions.onLifecycleChanged(oldContent, LifecycleState.Disappear, null);
            }
            _currentPrimaryItem = object;
            LifecycleMugenExtensions.onLifecycleChanging(object, LifecycleState.Appear, null, false);
            LifecycleMugenExtensions.onLifecycleChanged(object, LifecycleState.Appear, null);
        }
    }

    @Override
    public boolean isDiffUtilSupported() {
        return false;
    }

    @Override
    public void onItemChanged(int position) {
        notifyDataSetChanged();
    }

    @Override
    public void onItemInserted(int position) {
        notifyDataSetChanged();
    }

    @Override
    public void onItemMoved(int fromPosition, int toPosition) {
        notifyDataSetChanged();
    }

    @Override
    public void onItemRemoved(int position) {
        notifyDataSetChanged();
    }

    @Override
    public void onItemRangeChanged(int positionStart, int itemCount) {
        notifyDataSetChanged();
    }

    @Override
    public void onItemRangeInserted(int positionStart, int itemCount) {
        notifyDataSetChanged();
    }

    @Override
    public void onItemRangeRemoved(int positionStart, int itemCount) {
        notifyDataSetChanged();
    }

    @Override
    public void onReset() {
        notifyDataSetChanged();
        BindableMemberMugenExtensions.onMemberChanged(_viewPager, BindableMemberConstant.SelectedIndex, null);
        BindableMemberMugenExtensions.onMemberChanged(_viewPager, BindableMemberConstant.SelectedIndexEvent, null);
    }
}
