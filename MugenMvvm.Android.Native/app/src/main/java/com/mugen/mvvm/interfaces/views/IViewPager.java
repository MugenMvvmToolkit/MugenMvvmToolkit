package com.mugen.mvvm.interfaces.views;

public interface IViewPager extends IListView {
    String SelectedIndexName = "SelectedIndex";
    String SelectedIndexEventName = "SelectedIndexChanged";

    int getSelectedIndex();

    void setSelectedIndex(int index);

    void setSelectedIndex(int index, boolean smoothScroll);
}
