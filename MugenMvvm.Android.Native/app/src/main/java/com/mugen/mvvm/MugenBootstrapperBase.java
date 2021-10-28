package com.mugen.mvvm;

import android.content.ContentProvider;
import android.content.ContentValues;
import android.content.Context;
import android.content.pm.ProviderInfo;
import android.database.Cursor;
import android.net.Uri;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.mugen.mvvm.constants.LifecycleState;
import com.mugen.mvvm.constants.MugenInitializationFlags;
import com.mugen.mvvm.internal.AppStateDispatcher;
import com.mugen.mvvm.views.LifecycleMugenExtensions;

public abstract class MugenBootstrapperBase extends ContentProvider {
    private boolean _isInitialized;

    @Override
    public final boolean onCreate() {
        return true;
    }

    @Override
    public void attachInfo(Context context, ProviderInfo info) {
        if (initializeNative(context, info))
            initialize();
    }

    @Nullable
    @Override
    public final Cursor query(@NonNull Uri uri, @Nullable String[] projection, @Nullable String selection, @Nullable String[] selectionArgs, @Nullable String sortOrder) {
        throwNotSupported();
        return null;
    }

    @Nullable
    @Override
    public final String getType(@NonNull Uri uri) {
        throwNotSupported();
        return null;
    }

    @Nullable
    @Override
    public final Uri insert(@NonNull Uri uri, @Nullable ContentValues values) {
        throwNotSupported();
        return null;
    }

    @Override
    public final int delete(@NonNull Uri uri, @Nullable String selection, @Nullable String[] selectionArgs) {
        return throwNotSupported();
    }

    @Override
    public final int update(@NonNull Uri uri, @Nullable ContentValues values, @Nullable String selection, @Nullable String[] selectionArgs) {
        return throwNotSupported();
    }

    @Override
    public void onTrimMemory(int level) {
        super.onTrimMemory(level);
        if (level == TRIM_MEMORY_UI_HIDDEN)
            LifecycleMugenExtensions.onLifecycleChanged(MugenUtils.getAppContext(), LifecycleState.AppBackground, null);
    }

    protected final boolean initializeNative(Context context, ProviderInfo info) {
        if (_isInitialized)
            return false;
        _isInitialized = true;
        initializeNativeInternal(context, info);
        return true;
    }

    protected void initializeNativeInternal(Context context, ProviderInfo info) {
        MugenUtils.initializeCore(context, getFlags());
        if (MugenUtils.hasFlag(MugenInitializationFlags.NoAppState))
            MugenService.addLifecycleDispatcher(new AppStateDispatcher(getRootActivity()), false);
    }

    @Nullable
    protected String getRootActivity() {
        return null;
    }

    protected abstract int getFlags();

    protected abstract void initialize();

    private int throwNotSupported() {
        throw new RuntimeException("This operation is not supported.");
    }
}
