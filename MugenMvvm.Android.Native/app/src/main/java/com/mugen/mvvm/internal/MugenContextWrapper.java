package com.mugen.mvvm.internal;

import android.app.Activity;
import android.content.Context;
import android.content.ContextWrapper;
import android.util.AttributeSet;
import android.view.LayoutInflater;
import android.view.View;

public class MugenContextWrapper extends ContextWrapper {
    private MugenLayoutInflater mInflater;

    MugenContextWrapper(Context base) {
        super(base);
    }

    public static ContextWrapper wrap(Context base) {
        return new MugenContextWrapper(base);
    }

    public static View onActivityCreateView(Activity activity, View parent, View view, String name, Context context, AttributeSet attr) {
        return get(activity).onActivityCreateView(parent, view, name, context, attr);
    }

    static MugenLayoutInflater get(Activity activity) {
        if (!(activity.getLayoutInflater() instanceof MugenLayoutInflater)) {
            throw new RuntimeException("This activity does not wrap the Base Context! See MugenContextWrapper.wrap(Context)");
        }
        return (MugenLayoutInflater) activity.getLayoutInflater();
    }

    @Override
    public Object getSystemService(String name) {
        if (LAYOUT_INFLATER_SERVICE.equals(name)) {
            if (mInflater == null)
                mInflater = new MugenLayoutInflater(LayoutInflater.from(getBaseContext()), this, false);
            return mInflater;
        }
        return super.getSystemService(name);
    }
}