package com.mugen.mvvm.interfaces;

import androidx.annotation.NonNull;

import com.mugen.mvvm.internal.AttachedValues;

public interface IAttachedValueProvider {
    boolean isSupportAttachedValues(@NonNull Object target);

    AttachedValues getAttachedValues(@NonNull Object target, boolean required);
}
