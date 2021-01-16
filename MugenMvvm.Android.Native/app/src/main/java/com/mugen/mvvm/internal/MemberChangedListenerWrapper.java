package com.mugen.mvvm.internal;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.mugen.mvvm.interfaces.IMemberChangedListener;
import com.mugen.mvvm.interfaces.IMemberListener;

import java.util.HashMap;

public class MemberChangedListenerWrapper extends HashMap<String, IMemberListener> implements IMemberChangedListener {
    private IMemberChangedListener _listener;

    public MemberChangedListenerWrapper() {
        super(2);
    }

    @Nullable
    public IMemberChangedListener getListener() {
        return _listener;
    }

    public void setListener(@Nullable IMemberChangedListener listener) {
        _listener = listener;
    }

    public void onChanged(@NonNull Object sender, @NonNull CharSequence member, @Nullable Object args) {
        if (_listener != null)
            _listener.onChanged(sender, member, args);
    }
}
