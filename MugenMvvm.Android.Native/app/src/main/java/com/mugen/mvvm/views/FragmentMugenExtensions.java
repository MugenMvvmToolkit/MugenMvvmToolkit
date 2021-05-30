package com.mugen.mvvm.views;

import android.content.Context;
import android.os.Bundle;
import android.view.View;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.DialogFragment;
import androidx.fragment.app.Fragment;
import androidx.fragment.app.FragmentActivity;
import androidx.fragment.app.FragmentManager;
import androidx.fragment.app.FragmentTransaction;

import com.mugen.mvvm.MugenUtils;
import com.mugen.mvvm.interfaces.views.IActivityView;
import com.mugen.mvvm.interfaces.views.IDialogFragmentView;
import com.mugen.mvvm.interfaces.views.IFragmentView;
import com.mugen.mvvm.internal.ViewAttachedValues;

public final class FragmentMugenExtensions {
    private FragmentMugenExtensions() {
    }

    public static boolean isSupported(@Nullable Object fragment) {
        return MugenUtils.isCompatSupported() && fragment instanceof IFragmentView;
    }

    public static boolean isDestroyed(@NonNull IFragmentView fragment) {
        Fragment f = (Fragment) fragment.getFragment();
        FragmentActivity activity = f.getActivity();
        if (activity == null)
            return false;
        return activity.isDestroyed();
    }

    public static Context getActivity(@NonNull IFragmentView fragment) {
        return ((Fragment) fragment.getFragment()).getActivity();
    }

    @NonNull
    public static Object getFragmentOwner(@NonNull View container) {
        View v = container;
        while (v != null) {
            ViewAttachedValues attachedValues = ViewMugenExtensions.getNativeAttachedValues(v, false);
            if (attachedValues != null) {
                Fragment fragment = (Fragment) attachedValues.getFragment();
                if (fragment != null)
                    return fragment;
            }

            Object parent = BindableMemberMugenExtensions.getParent(v);
            if (parent instanceof View)
                v = (View) parent;
            else
                v = null;
        }

        //noinspection ConstantConditions
        return ActivityMugenExtensions.tryGetActivity(container.getContext());
    }

    public static Object getFragmentManager(@NonNull Object owner) {
        if (owner instanceof View)
            owner = getFragmentOwner((View) owner);
        if (owner instanceof FragmentActivity)
            return ((FragmentActivity) owner).getSupportFragmentManager();
        if (owner instanceof Fragment)
            return ((Fragment) owner).getChildFragmentManager();
        if (owner instanceof IActivityView)
            return ((FragmentActivity) ((IActivityView) owner).getActivity()).getSupportFragmentManager();
        return ((Fragment) ((IFragmentView) owner).getFragment()).getChildFragmentManager();
    }

    public static boolean setFragment(@NonNull View container, @Nullable IFragmentView target) {
        Fragment fragment = (Fragment) (target == null ? null : target.getFragment());
        FragmentManager fragmentManager = (FragmentManager) getFragmentManager(container);
        if (fragment == null) {
            Fragment oldFragment = fragmentManager.findFragmentById(container.getId());
            if (oldFragment != null && !fragmentManager.isDestroyed()) {
                fragmentManager.beginTransaction().remove(oldFragment).commitNowAllowingStateLoss();
                return true;
            }
            return false;
        }

        FragmentTransaction fragmentTransaction = fragmentManager.beginTransaction();
        if (fragment.isDetached())
            fragmentTransaction.attach(fragment);
        else
            fragmentTransaction.replace(container.getId(), fragment);
        fragmentTransaction.commitNowAllowingStateLoss();
        return true;
    }

    public static void show(@NonNull IDialogFragmentView fragmentView, @NonNull IActivityView activityView, @Nullable String tag) {
        DialogFragment fragment = (DialogFragment) fragmentView.getFragment();
        FragmentActivity activity = (FragmentActivity) activityView.getActivity();
        fragment.show(activity.getSupportFragmentManager(), tag);
    }

    public static void clearFragmentState(@NonNull Bundle bundle) {
        bundle.remove("android:support:fragments");
        bundle.remove("android:fragments");
    }

    public static void remove(@NonNull IFragmentView fragmentView) {
        Fragment fragment = (Fragment) fragmentView.getFragment();
        FragmentManager fragmentManager = fragment.getFragmentManager();
        if (fragmentManager != null)
            fragmentManager.beginTransaction().remove(fragment).commitAllowingStateLoss();
    }
}
