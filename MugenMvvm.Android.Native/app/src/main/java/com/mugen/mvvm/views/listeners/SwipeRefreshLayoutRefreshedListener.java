package com.mugen.mvvm.views.listeners;

import androidx.annotation.NonNull;
import androidx.swiperefreshlayout.widget.SwipeRefreshLayout;

import com.mugen.mvvm.constants.BindableMemberConstant;
import com.mugen.mvvm.interfaces.IMemberListener;
import com.mugen.mvvm.views.BindableMemberMugenExtensions;

public class SwipeRefreshLayoutRefreshedListener implements SwipeRefreshLayout.OnRefreshListener, IMemberListener {
    private final SwipeRefreshLayout _refreshLayout;
    private short _listenerCount;

    public SwipeRefreshLayoutRefreshedListener(Object refreshLayout) {
        _refreshLayout = (SwipeRefreshLayout) refreshLayout;
    }

    @Override
    public void onRefresh() {
        BindableMemberMugenExtensions.onMemberChanged(_refreshLayout, BindableMemberConstant.RefreshedEvent, null);
    }

    @Override
    public void addListener(@NonNull Object target, @NonNull String memberName) {
        if (BindableMemberConstant.RefreshedEvent.equals(memberName) && _listenerCount++ == 0)
            _refreshLayout.setOnRefreshListener(this);
    }

    @Override
    public void removeListener(@NonNull Object target, @NonNull String memberName) {
        if (BindableMemberConstant.RefreshedEvent.equals(memberName) && --_listenerCount == 0)
            _refreshLayout.setOnRefreshListener(null);
    }
}
