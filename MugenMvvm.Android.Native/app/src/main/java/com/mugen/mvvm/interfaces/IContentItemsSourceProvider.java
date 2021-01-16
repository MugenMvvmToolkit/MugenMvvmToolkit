package com.mugen.mvvm.interfaces;

import androidx.annotation.NonNull;

public interface IContentItemsSourceProvider extends IItemsSourceProviderBase {
    int POSITION_UNCHANGED = -1;
    int POSITION_NONE = -2;

    @NonNull
    Object getContent(int position);

    int getContentPosition(@NonNull Object content);
}
