package com.mugen.mvvm.constants;

public final class BindableMemberConstant {
    public static final int SelectedIndexNotSupported = Integer.MIN_VALUE + 1;

    public static final CharSequence Parent = "Parent";
    public static final CharSequence ParentEvent = "ParentChanged";
    public static final CharSequence Click = "Click";
    public static final CharSequence LongClick = "LongClick";
    public static final CharSequence Text = "Text";
    public static final CharSequence TextEvent = "TextChanged";
    public static final CharSequence HomeButtonClick = "HomeButtonClick";
    public static final CharSequence RefreshedEvent = "Refreshed";
    public final static CharSequence SelectedIndex = "SelectedIndex";
    public final static CharSequence SelectedIndexEvent = "SelectedIndexChanged";
    public static final CharSequence Checked = "Checked";
    public static final CharSequence CheckedEvent = "CheckedChanged";

    private BindableMemberConstant() {
    }
}
