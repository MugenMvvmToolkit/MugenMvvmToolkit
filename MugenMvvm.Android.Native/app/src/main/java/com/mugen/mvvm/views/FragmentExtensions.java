package com.mugen.mvvm.views;

import android.content.Context;
import android.os.BaseBundle;
import android.view.View;
import androidx.annotation.Nullable;
import androidx.fragment.app.*;
import com.mugen.mvvm.MugenNativeService;
import com.mugen.mvvm.interfaces.views.IActivityView;
import com.mugen.mvvm.interfaces.views.IDialogFragmentView;
import com.mugen.mvvm.interfaces.views.IFragmentView;
import com.mugen.mvvm.internal.ViewAttachedValues;

public abstract class FragmentExtensions {
    public static boolean isSupported(Object fragment) {
        return MugenNativeService.isCompatSupported() && fragment instanceof IFragmentView;
    }

    public static Context getActivity(IFragmentView fragment) {
        return ((Fragment) fragment.getFragment()).getActivity();
    }

    public static Object getFragmentManager(View container) {
        View v = container;
        while (v != null) {
            ViewAttachedValues attachedValues = ViewExtensions.getNativeAttachedValues(v, false);
            if (attachedValues != null) {
                Fragment fragment = attachedValues.getFragment();
                if (fragment != null)
                    return fragment.getFragmentManager();
            }
            Object parent = ViewExtensions.getParent(v);
            if (parent instanceof View)
                v = (View) parent;
            else
                v = null;
        }

        FragmentActivity activity = (FragmentActivity) ActivityExtensions.getActivity(container.getContext());
        return activity.getSupportFragmentManager();
    }

    public static boolean setFragment(View container, IFragmentView target) {
        Fragment fragment = (Fragment) (target == null ? null : target.getFragment());
        FragmentManager fragmentManager = (FragmentManager) getFragmentManager(container);
        if (fragment == null) {
            Fragment oldFragment = fragmentManager.findFragmentById(container.getId());
            if (oldFragment != null && !fragmentManager.isDestroyed()) {
                fragmentManager.beginTransaction().remove(oldFragment).commitAllowingStateLoss();
                fragmentManager.executePendingTransactions();
                return true;
            }
            return false;
        }

        FragmentTransaction fragmentTransaction = fragmentManager.beginTransaction();
        if (fragment.isDetached())
            fragmentTransaction.attach(fragment);
        else
            fragmentTransaction.replace(container.getId(), fragment);
        fragmentTransaction.commit();
        fragmentManager.executePendingTransactions();
        return true;
    }

    public static void show(IDialogFragmentView fragmentView, IActivityView activityView, @Nullable String tag) {
        DialogFragment fragment = (DialogFragment) fragmentView.getFragment();
        FragmentActivity activity = (FragmentActivity) activityView.getActivity();
        fragment.show(activity.getSupportFragmentManager(), tag);
    }

    public static void clearFragmentState(BaseBundle bundle) {
        bundle.remove("android:support:fragments");
        bundle.remove("android:fragments");
    }

    public static void remove(IFragmentView fragmentView) {
        Fragment fragment = (Fragment) fragmentView.getFragment();
        FragmentManager fragmentManager = fragment.getFragmentManager();
        if (fragmentManager != null)
            fragmentManager.beginTransaction().remove(fragment).commitAllowingStateLoss();
    }
}
