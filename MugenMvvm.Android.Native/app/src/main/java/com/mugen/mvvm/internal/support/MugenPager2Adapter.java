package com.mugen.mvvm.internal.support;

import android.view.View;

import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;
import androidx.viewpager2.widget.ViewPager2;

import com.mugen.mvvm.constants.LifecycleState;
import com.mugen.mvvm.interfaces.IResourceItemsSourceProvider;
import com.mugen.mvvm.interfaces.views.IHasLifecycleView;
import com.mugen.mvvm.views.LifecycleExtensions;

public class MugenPager2Adapter extends MugenRecyclerViewAdapter {
    private final ViewPager2 _viewPager;
    private Listener _listener;
    private RecyclerView _recyclerView;

    public MugenPager2Adapter(ViewPager2 viewPager2, IResourceItemsSourceProvider provider) {
        super(viewPager2.getContext(), provider);
        _viewPager = viewPager2;
    }

    @Override
    public void attach(@NonNull RecyclerView recyclerView) {
        super.attach(recyclerView);
        _recyclerView = recyclerView;
        if (_listener == null)
            _listener = new Listener();
        _viewPager.registerOnPageChangeCallback(_listener);
    }

    @Override
    public void detach() {
        super.detach();
        if (_listener != null)
            _viewPager.unregisterOnPageChangeCallback(_listener);
        _recyclerView = null;
    }

    @Override
    public void onViewAttachedToWindow(@NonNull RecyclerView.ViewHolder holder) {
        super.onViewAttachedToWindow(holder);
        if (_listener != null)
            _listener.updateIfNeed();
    }

    private final class Listener extends ViewPager2.OnPageChangeCallback {
        private int _selectedIndex;
        private boolean _isFirstPageNotified;

        public Listener() {
            _selectedIndex = -1;
        }

        public void updateIfNeed() {
            if (!_isFirstPageNotified)
                updateSelectedIndex(_viewPager.getCurrentItem());
        }

        @Override
        public void onPageSelected(int position) {
            if (position != _selectedIndex)
                updateSelectedIndex(position);
        }

        private void updateSelectedIndex(int position) {
            View oldPage = tryGetItem(_selectedIndex);
            View newPage = tryGetItem(position);
            _selectedIndex = position;

            if (oldPage != null && oldPage != newPage && !(oldPage instanceof IHasLifecycleView)) {
                LifecycleExtensions.onLifecycleChanging(oldPage, LifecycleState.Pause, null);
                LifecycleExtensions.onLifecycleChanged(oldPage, LifecycleState.Pause, null);
            }

            if (newPage != null && !(newPage instanceof IHasLifecycleView)) {
                _isFirstPageNotified = true;
                LifecycleExtensions.onLifecycleChanging(newPage, LifecycleState.Resume, null);
                LifecycleExtensions.onLifecycleChanged(newPage, LifecycleState.Resume, null);
            }
        }

        private View tryGetItem(int position) {
            if (position < 0 || _recyclerView == null || getItemCount() <= position)
                return null;
            RecyclerView.LayoutManager layoutManager = _recyclerView.getLayoutManager();
            if (layoutManager == null)
                return null;
            return layoutManager.findViewByPosition(position);
        }
    }
}
