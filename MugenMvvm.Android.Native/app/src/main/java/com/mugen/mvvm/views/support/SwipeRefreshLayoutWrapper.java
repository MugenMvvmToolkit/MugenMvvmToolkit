package com.mugen.mvvm.views.support;

import android.view.View;
import androidx.swiperefreshlayout.widget.SwipeRefreshLayout;
import com.mugen.mvvm.interfaces.views.IRefreshView;
import com.mugen.mvvm.views.ViewWrapper;

public class SwipeRefreshLayoutWrapper extends ViewWrapper implements IRefreshView, SwipeRefreshLayout.OnRefreshListener {
    private int _listenerCount;

    public SwipeRefreshLayoutWrapper(Object view) {
        super(view);
    }

    @Override
    protected void addMemberListener(View view, String memberName) {
        super.addMemberListener(view, memberName);
        if (RefreshedEventName.equals(memberName) && _listenerCount++ == 0)
            ((SwipeRefreshLayout) view).setOnRefreshListener(this);
    }

    @Override
    protected void removeMemberListener(View view, String memberName) {
        super.removeMemberListener(view, memberName);
        if (RefreshedEventName.equals(memberName) && --_listenerCount == 0)
            ((SwipeRefreshLayout) view).setOnRefreshListener(null);
    }

    @Override
    public void onRefresh() {
        onMemberChanged(RefreshedEventName, null);
    }
}
