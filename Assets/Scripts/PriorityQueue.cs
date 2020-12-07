using System;
using System.Collections.Generic;

public class PriorityQueue<T>
{
    private readonly List<T> dataHeap;
    private readonly List<float> priorities;

    /// <summary>
    /// Initializes a new instance of the Priority Queue that is empty.
    /// </summary>
    public PriorityQueue()
    {
        this.dataHeap = new List<T>();
        this.priorities = new List<float>();
    }

    public void Enqueue(T value, float priority)
    {
        this.dataHeap.Add(value);
        this.priorities.Add(priority);
        BubbleUp();
    }

    public T Dequeue()
    {
        if (this.dataHeap.Count <= 0)
            throw new InvalidOperationException("Cannot Dequeue from empty queue!");

        T result = dataHeap[0];
        int count = this.dataHeap.Count - 1;
        dataHeap[0] = dataHeap[count];
        priorities[0] = priorities[count];
        dataHeap.RemoveAt(count);
        priorities.RemoveAt(count);
        ShiftDown();

        return result;
    }

    /// <summary>
    /// A method to maintain the heap order of the elements after enqueue. If the parent of the newly added 
    /// element is with less priority - swap them.
    /// </summary>
    private void BubbleUp()
    {
        int childIndex = dataHeap.Count - 1;

        while (childIndex > 0)
        {
            int parentIndex = (childIndex - 1) / 2;

            if (priorities[childIndex] >= priorities[parentIndex]) break;

            SwapAt(childIndex, parentIndex);
            childIndex = parentIndex;
        }
    }

    /// <summary>
    /// A method to maintain the heap order of the elements after denqueue. We check priorities of both children and parent node.
    /// </summary>
    private void ShiftDown()
    {
        int count = this.dataHeap.Count - 1;
        int parentIndex = 0;

        while (true)
        {
            int childIndex = parentIndex * 2 + 1;
            if (childIndex > count) break;

            int rightChild = childIndex + 1;
            if (rightChild <= count && priorities[rightChild] < priorities[childIndex]) childIndex = rightChild;

            if (priorities[parentIndex] <= priorities[childIndex]) break;

            SwapAt(parentIndex, childIndex);
            parentIndex = childIndex;
        }
    }

    /// <summary>Returns the element at the front of the Priority Queue without removing it.</summary>
    public T Peek()
    {
        if (this.dataHeap.Count == 0)
            throw new InvalidOperationException("Queue is empty");

        T frontItem = dataHeap[0];
        return frontItem;
    }

    public int Count { get => dataHeap.Count; }

    /// <summary>Removes all elements from the queue.</summary>
    public void Clear()
    {
        this.dataHeap.Clear();
    }

    private void SwapAt(int first, int second)
    {
        T value = dataHeap[first];
        float priority = priorities[first];

        dataHeap[first] = dataHeap[second];
        priorities[first] = priorities[second];

        dataHeap[second] = value;
        priorities[second] = priority;
    }
}