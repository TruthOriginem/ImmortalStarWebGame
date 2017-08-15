using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public struct lint : IComparable<lint>
{

    private long value;

    private lint(int value)
    {
        this.value = value;
    }
    private lint(long value)
    {
        this.value = value;
    }

    public static implicit operator lint(int value)
    {
        return new lint(value);
    }

    public static implicit operator lint(long value)
    {
        return new lint(value);
    }
    public static implicit operator int(lint record)
    {
        return (int)record.value;
    }
    public static implicit operator long(lint record)
    {
        return record.value;
    }
    public override string ToString()
    {
        return value.ToString();
    }

    public int CompareTo(lint other)
    {
        return value.CompareTo(other);
    }
}
