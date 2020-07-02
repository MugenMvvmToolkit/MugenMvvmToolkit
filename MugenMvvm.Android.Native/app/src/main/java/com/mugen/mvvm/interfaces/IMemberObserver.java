package com.mugen.mvvm.interfaces;

public interface IMemberObserver {
    void onMemberChanged(Object sender, String member, Object args);
}
