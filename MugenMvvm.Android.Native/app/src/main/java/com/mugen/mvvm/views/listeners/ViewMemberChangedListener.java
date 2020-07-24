package com.mugen.mvvm.views.listeners;

import com.mugen.mvvm.interfaces.IMemberChangedListener;
import com.mugen.mvvm.views.ViewExtensions;

import java.util.HashMap;

public class ViewMemberChangedListener extends HashMap<String, ViewExtensions.IMemberListener> implements IMemberChangedListener {
    private IMemberChangedListener _memberObserver;

    public ViewMemberChangedListener() {
        super(2);
    }

    public IMemberChangedListener getListener() {
        return _memberObserver;
    }

    public void setListener(IMemberChangedListener memberObserver) {
        _memberObserver = memberObserver;
    }

    public void onChanged(Object sender, String member, Object args) {
        if (_memberObserver != null)
            _memberObserver.onChanged(sender, member, args);
    }
}
