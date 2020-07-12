package com.mugen.mvvm.internal.support;

import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.viewpager.widget.PagerAdapter;
import androidx.viewpager.widget.ViewPager;
import com.mugen.mvvm.interfaces.IContentItemsSourceProvider;
import com.mugen.mvvm.interfaces.IItemsSourceObserver;

//todo handle fragments
public class MugenPagerAdapter extends PagerAdapter implements IItemsSourceObserver {
    private final ViewPager _owner;
    private final IContentItemsSourceProvider _provider;
    private Object _currentPrimaryItem;

    public MugenPagerAdapter(ViewPager owner, IContentItemsSourceProvider provider) {
        _owner = owner;
        _provider = provider;
        provider.addObserver(this);
    }

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
        return _provider.getTitle(position);
    }

    @NonNull
    @Override
    public Object instantiateItem(@NonNull ViewGroup container, int position) {
        Object content = _provider.getContent(position);
        if (content == null) {
            TextView txt = new TextView(_owner.getContext());
            txt.setText("null");
            content = txt;
        }

        container.addView((View) content);
        return content;
    }

    @Override
    public void destroyItem(@NonNull ViewGroup container, int position, @NonNull Object object) {
        container.removeView((View) object);
        _provider.destroyContent(position, object);
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
            _currentPrimaryItem = object;
            _provider.onPrimaryContentChanged(position, oldContent, object);
        }
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
    }
}
