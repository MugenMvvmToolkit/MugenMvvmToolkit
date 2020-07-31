package com.mugen.mvvm.interfaces.views;

public interface IFragmentView extends IResourceView, IHasStateView, IHasLifecycleView {
    Object getFragment();
}
