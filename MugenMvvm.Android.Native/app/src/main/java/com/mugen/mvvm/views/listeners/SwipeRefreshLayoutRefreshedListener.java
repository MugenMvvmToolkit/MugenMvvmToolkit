package com.mugen.mvvm.views.listeners;

import androidx.swiperefreshlayout.widget.SwipeRefreshLayout;

import com.mugen.mvvm.views.ViewExtensions;

public class SwipeRefreshLayoutRefreshedListener implements SwipeRefreshLayout.OnRefreshListener, ViewExtensions.IMemberListener {
    private final SwipeRefreshLayout _refreshLayout;
    private short _listenerCount;

    public SwipeRefreshLayoutRefreshedListener(Object refreshLayout) {
        _refreshLayout = (SwipeRefreshLayout) refreshLayout;
    }

    @Override
    public void onRefresh() {
        ViewExtensions.onMemberChanged(_refreshLayout, ViewExtensions.RefreshedEventName, null);
    }

    @Override
    public void addListener(Object target, String memberName) {
        if (ViewExtensions.RefreshedEventName.equals(memberName) && _listenerCount++ == 0)
            _refreshLayout.setOnRefreshListener(this);
    }

    @Override
    public void removeListener(Object target, String memberName) {
        if (ViewExtensions.RefreshedEventName.equals(memberName) && --_listenerCount == 0)
            _refreshLayout.setOnRefreshListener(null);
    }
}
