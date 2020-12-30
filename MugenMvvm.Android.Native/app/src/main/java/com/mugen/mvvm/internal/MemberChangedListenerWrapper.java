package com.mugen.mvvm.internal;

import com.mugen.mvvm.interfaces.IMemberChangedListener;
import com.mugen.mvvm.views.ViewMugenExtensions;

import java.util.HashMap;

public class MemberChangedListenerWrapper extends HashMap<String, ViewMugenExtensions.IMemberListener> implements IMemberChangedListener {
    private IMemberChangedListener _listener;

    public MemberChangedListenerWrapper() {
        super(2);
    }

    public IMemberChangedListener getListener() {
        return _listener;
    }

    public void setListener(IMemberChangedListener listener) {
        _listener = listener;
    }

    public void onChanged(Object sender, CharSequence member, Object args) {
        if (_listener != null)
            _listener.onChanged(sender, member, args);
    }
}
