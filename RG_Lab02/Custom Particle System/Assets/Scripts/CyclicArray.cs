using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CyclicArray<T>
{
    public int Count { get; private set; }
    public int Capacity { get; private set; }

    private int _initialCapacity;

    private T[] _array;

    private int _first;
    private int _last;

    public CyclicArray(int capacity)
    {
        Capacity = _initialCapacity = capacity;
        _first = 0;
        _last = 0;
        _array = new T[capacity];
    }

    public bool IsEmpty => _first == _last;

    public void Add(T el)
    {
        if (Count >= Capacity)
        {
            Expand(_initialCapacity);
        }

        _array[_last] = el;
        _last++;
        _last %= _array.Length;
        Count++;
    }

    public void Pop()
    {
        if (Count <= 0)
            return;

        Count--;
        _first++;
        _first %= _array.Length;
    }

    public T this[int idx]
    {
        get { return _array[(_first + idx) % _array.Length]; }

        set
        {
            _array[(_first + idx) % _array.Length] = value;
        }
    }

    public void Expand(int howMuch)
    {
        if (_last < _first)
            howMuch = Mathf.Max(howMuch, _last);

        int newCapacity = Capacity + howMuch;
        System.Array.Resize(ref _array, newCapacity);

        if (_last < _first)
            System.Array.Copy(_array, 0, _array, Capacity, _last);

        Capacity += howMuch;
    }
}
