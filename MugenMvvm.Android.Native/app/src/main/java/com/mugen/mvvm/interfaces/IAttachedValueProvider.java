package com.mugen.mvvm.interfaces;

import com.mugen.mvvm.internal.AttachedValues;

public interface IAttachedValueProvider {
    AttachedValues getAttachedValues(Object target, boolean required);
}
