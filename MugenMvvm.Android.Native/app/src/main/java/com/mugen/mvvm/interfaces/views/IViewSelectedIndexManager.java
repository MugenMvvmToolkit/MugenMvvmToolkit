package com.mugen.mvvm.interfaces.views;

import androidx.annotation.NonNull;

public interface IViewSelectedIndexManager {
    boolean isSelectedIndexSupported(@NonNull Object view);

    int getSelectedIndex(@NonNull Object view);

    void setSelectedIndex(@NonNull Object view, int index, boolean smoothScroll);
}
