package com.mugen.mvvm.interfaces;

public interface IContentItemsSourceProvider extends IItemsSourceProviderBase {
    int POSITION_UNCHANGED = -1;
    int POSITION_NONE = -2;

    CharSequence getTitle(int position);

    Object getContent(int position);

    int getContentPosition(Object content);

    void onPrimaryContentChanged(int position, Object oldContent, Object newContent);

    void destroyContent(int position, Object content);
}
