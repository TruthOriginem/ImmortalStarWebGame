using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Currency
{
    // A struct's fields should not be exposed
    private long value;

    // As we are using implicit conversions we can keep the constructor private
    private Currency(int value)
    {
        this.value = value;
    }
    private Currency(long value)
    {
        this.value = value;
    }

    public static implicit operator Currency(int value)
    {
        return new Currency(value);
    }

    public static implicit operator Currency(long value)
    {
        return new Currency(value);
    }
    public static implicit operator int(Currency record)
    {
        return (int)record.value;
    }
    public static implicit operator long(Currency record)
    {
        return record.value;
    }
    public override string ToString()
    {
        return value.ToString();
    }
}
