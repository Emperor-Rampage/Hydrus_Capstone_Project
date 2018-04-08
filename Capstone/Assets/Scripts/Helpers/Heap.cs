using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHeapObject<T> : IComparable<T>
{
    int HeapIndex { get; set; }
}
public class Heap<T> where T : IHeapObject<T>
{
    T[] objects;
    int count;

    public int Count { get { return count; } }

    public Heap(int maxSize)
    {
        objects = new T[maxSize];
    }
    public void BubbleItem(T obj)
    {
        BubbleUp(obj);
        //        BubbleDown(obj);
    }
    public void Add(T obj)
    {
        obj.HeapIndex = count;
        objects[count++] = obj;

        BubbleItem(obj);
    }
    public T RemoveFirst()
    {
        T obj = objects[0];
        count--;
        objects[0] = objects[count];
        objects[0].HeapIndex = 0;
        BubbleDown(objects[0]);

        return obj;
    }
    public bool Contains(T obj)
    {
        return Equals(objects[obj.HeapIndex], obj);
    }
    void BubbleUp(T obj)
    {
        int parentIndex = (obj.HeapIndex - 1) / 2;

        while (true)
        {
            T parentObj = objects[parentIndex];
            if (obj.CompareTo(parentObj) > 0)
                Swap(obj, parentObj);
            else
                break;

            parentIndex = (obj.HeapIndex - 1) / 2;
        }
    }
    void BubbleDown(T obj)
    {
        while (true)
        {
            int indexLeft = (obj.HeapIndex * 2) + 1;
            int indexRight = (obj.HeapIndex * 2) + 2;
            int swapIndex = 0;

            if (indexLeft < count)
            {
                swapIndex = indexLeft;
                if (indexRight < count)
                {
                    if (objects[indexLeft].CompareTo(objects[indexRight]) < 0)
                    {
                        swapIndex = indexRight;
                    }
                }

                if (obj.CompareTo(objects[swapIndex]) < 0)
                    Swap(obj, objects[swapIndex]);
                else
                    break;
            }
            else
            {
                break;
            }
        }
    }
    void Swap(T obj1, T obj2)
    {
        int temp = obj1.HeapIndex;
        obj1.HeapIndex = obj2.HeapIndex;
        obj2.HeapIndex = temp;

        objects[obj1.HeapIndex] = obj1;
        objects[obj2.HeapIndex] = obj2;
    }
}
