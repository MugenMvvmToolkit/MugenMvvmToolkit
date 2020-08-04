package com.mugen.mvvm.interfaces;

public interface IContentItemsSourceProvider extends IItemsSourceProviderBase {
    int POSITION_UNCHANGED = -1;
    int POSITION_NONE = -2;

    Object getContent(int position);

    int getContentPosition(Object content);
}
