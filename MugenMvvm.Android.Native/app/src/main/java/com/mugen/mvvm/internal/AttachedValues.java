package com.mugen.mvvm.internal;

import android.util.SparseArray;
import com.mugen.mvvm.interfaces.IMemberChangedListener;

public class AttachedValues {
    private Object _attachedValues;
    private MemberChangedListenerWrapper _memberListener;
    private SparseArray<Object> _tags;

    public IMemberChangedListener getMemberListener() {
        if (_memberListener == null)
            return null;
        return _memberListener.getListener();
    }

    public MemberChangedListenerWrapper getMemberListenerWrapper(boolean required) {
        if (_memberListener == null && required)
            _memberListener = new MemberChangedListenerWrapper();
        return _memberListener;
    }

    public void setMemberListener(IMemberChangedListener listener) {
        if (listener == null && _memberListener == null)
            return;
        if (_memberListener == null)
            _memberListener = new MemberChangedListenerWrapper();
        _memberListener.setListener(listener);
    }

    public Object getAttachedValues() {
        return _attachedValues;
    }

    public void setAttachedValues(Object attachedValues) {
        _attachedValues = attachedValues;
    }

    public Object getTag(int id) {
        if (_tags == null)
            return null;
        return _tags.get(id);
    }

    public void setTag(int id, Object value) {
        if (_tags == null)
            _tags = new SparseArray<>(2);
        if (value == null)
            _tags.remove(id);
        else
            _tags.put(id, value);
    }
}
