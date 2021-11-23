package com.mugen.mvvm.interfaces.views;

import android.content.Intent;
import android.os.Bundle;

import androidx.annotation.Nullable;

public interface IViewLayoutResourceResolver {
    @Nullable
    Class tryGetClassByLayoutId(int resourceId, boolean isActivity, @Nullable Bundle metadata);

    int tryGetLayoutId(@Nullable Class viewClass, @Nullable Intent intent, @Nullable Bundle metadata);
}
