package com.mugen.mvvm.internal;

import com.mugen.mvvm.interfaces.IHasPriority;

import java.util.Comparator;

public final class HasPriorityComparator implements Comparator<IHasPriority> {
    public static final HasPriorityComparator Instance = new HasPriorityComparator();

    private HasPriorityComparator() {
    }

    @Override
    public int compare(IHasPriority o1, IHasPriority o2) {
        return Integer.compare(o2.getPriority(), o1.getPriority());
    }
}
