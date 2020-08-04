//package com.mugen.mvvm.views.fragments;
//
//import android.app.Activity;
//import android.app.Dialog;
//import android.content.Context;
//import android.content.DialogInterface;
//import android.os.Bundle;
//import android.view.*;
//import androidx.annotation.NonNull;
//import androidx.annotation.Nullable;
//import com.mugen.mvvm.interfaces.views.IDialogFragmentView;
//import com.mugen.mvvm.interfaces.views.INativeFragmentView;
//
//public class MugenDialogFragmentLite implements INativeFragmentView, IDialogFragmentView {
//    @Override
//    public Object getFragment() {
//        return this;
//    }
//
//    @Override
//    public int getViewId() {
//        return 0;
//    }
//
//    @Override
//    public Object getState() {
//        return null;
//    }
//
//    @Override
//    public void setState(Object tag) {
//    }
//
//    @Nullable
//    public Context getContext() {
//        return null;
//    }
//
//    final public Activity getActivity() {
//        return null;
//    }
//
//    @Nullable
//    public View getView() {
//        return null;
//    }
//
//    public void onCreate(@Nullable Bundle savedInstanceState) {
//    }
//
//    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
//        return null;
//    }
//
//    public void onDestroyView() {
//    }
//
//    public void onDestroy() {
//    }
//
//    public void onPause() {
//    }
//
//    public void onResume() {
//    }
//
//    public void onSaveInstanceState(@NonNull Bundle outState) {
//    }
//
//    public void onStart() {
//    }
//
//    public void onStop() {
//    }
//
//    public boolean isCancelable() {
//        return false;
//    }
//
//    @Override
//    public void setCancelable(boolean cancelable) {
//    }
//
//    @Override
//    public void dismiss() {
//    }
//
//    @Override
//    public void dismissAllowingStateLoss() {
//    }
//
//    public void onCancel(@NonNull DialogInterface dialog) {
//    }
//
//    public void setupDialog(@NonNull Dialog dialog, int style) {
//    }
//
//    public Dialog onCreateDialog(@Nullable Bundle savedInstanceState) {
//        return null;
//    }
//
//    public void setStyle(int style, int theme) {
//    }
//
//    public Dialog getDialog() {
//        return null;
//    }
//
//    public int getTheme() {
//        return 0;
//    }
//}
