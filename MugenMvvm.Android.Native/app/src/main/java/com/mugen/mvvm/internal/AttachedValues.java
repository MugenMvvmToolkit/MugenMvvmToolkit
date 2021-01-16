package com.mugen.mvvm.internal;

import android.util.SparseArray;

import androidx.annotation.Nullable;

import com.mugen.mvvm.interfaces.IMemberChangedListener;

public class AttachedValues {
    private Object _attachedValues;
    private MemberChangedListenerWrapper _memberListener;
    private SparseArray<Object> _tags;

    @Nullable
    public IMemberChangedListener getMemberListener() {
        if (_memberListener == null)
            return null;
        return _memberListener.getListener();
    }

    public void setMemberListener(@Nullable IMemberChangedListener listener) {
        if (listener == null && _memberListener == null)
            return;
        if (_memberListener == null)
            _memberListener = new MemberChangedListenerWrapper();
        _memberListener.setListener(listener);
    }

    public MemberChangedListenerWrapper getMemberListenerWrapper(boolean required) {
        if (_memberListener == null && required)
            _memberListener = new MemberChangedListenerWrapper();
        return _memberListener;
    }

    @Nullable
    public Object getAttachedValues() {
        return _attachedValues;
    }

    public void setAttachedValues(@Nullable Object attachedValues) {
        _attachedValues = attachedValues;
    }

    @Nullable
    public Object getTag(int id) {
        if (_tags == null)
            return null;
        return _tags.get(id);
    }

    public void setTag(int id, @Nullable Object value) {
        if (_tags == null)
            _tags = new SparseArray<>(2);
        if (value == null)
            _tags.remove(id);
        else
            _tags.put(id, value);
    }
}
