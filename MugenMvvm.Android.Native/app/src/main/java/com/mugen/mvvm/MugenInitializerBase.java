package com.mugen.mvvm;

import android.content.ContentProvider;
import android.content.ContentValues;
import android.content.Context;
import android.content.pm.ProviderInfo;
import android.database.Cursor;
import android.net.Uri;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

public abstract class MugenInitializerBase extends ContentProvider {
    @Override
    public final boolean onCreate() {
        return true;
    }

    @Override
    public final void attachInfo(Context context, ProviderInfo info) {
        super.attachInfo(context, info);
        MugenUtils.setAppContext(context);
        initialize();
    }

    @Nullable
    @Override
    public final Cursor query(@NonNull Uri uri, @Nullable String[] projection, @Nullable String selection, @Nullable String[] selectionArgs, @Nullable String sortOrder) {
        throw new RuntimeException("This operation is not supported.");
    }

    @Nullable
    @Override
    public final String getType(@NonNull Uri uri) {
        throw new RuntimeException("This operation is not supported.");
    }

    @Nullable
    @Override
    public final Uri insert(@NonNull Uri uri, @Nullable ContentValues values) {
        throw new RuntimeException("This operation is not supported.");
    }

    @Override
    public final int delete(@NonNull Uri uri, @Nullable String selection, @Nullable String[] selectionArgs) {
        throw new RuntimeException("This operation is not supported.");
    }

    @Override
    public final int update(@NonNull Uri uri, @Nullable ContentValues values, @Nullable String selection, @Nullable String[] selectionArgs) {
        throw new RuntimeException("This operation is not supported.");
    }

    @Override
    public final void onTrimMemory(int level) {
        super.onTrimMemory(level);
        onTrimMemoryInternal(level);
    }

    protected abstract void initialize();

    protected abstract void onTrimMemoryInternal(int level);
}
