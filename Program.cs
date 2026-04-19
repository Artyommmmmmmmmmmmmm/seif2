using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Узел двусвязного списка
/// </summary>
public class LinkedListNode<T>
{
    public T Data { get; set; }
    public LinkedListNode<T> Prev { get; set; }
    public LinkedListNode<T> Next { get; set; }

    public LinkedListNode(T data)
    {
        Data = data;
        Prev = null;
        Next = null;
    }
}

/// <summary>
/// Двусвязный список с базовыми операциями
/// </summary>
public class DoublyLinkedList<T> : IEnumerable<T>
{
    private LinkedListNode<T> head;
    private LinkedListNode<T> tail;
    private int count;

    public int Count => count;
    public bool IsEmpty => count == 0;
    public LinkedListNode<T> First => head;
    public LinkedListNode<T> Last => tail;

    // --- БАЗОВЫЕ ОПЕРАЦИИ ---

    /// <summary>
    /// Добавление в начало списка
    /// </summary>
    public void AddFirst(T data)
    {
        LinkedListNode<T> newNode = new LinkedListNode<T>(data);

        if (IsEmpty)
        {
            head = tail = newNode;
        }
        else
        {
            newNode.Next = head;
            head.Prev = newNode;
            head = newNode;
        }
        count++;
    }

    /// <summary>
    /// Добавление в конец списка
    /// </summary>
    public void AddLast(T data)
    {
        LinkedListNode<T> newNode = new LinkedListNode<T>(data);

        if (IsEmpty)
        {
            head = tail = newNode;
        }
        else
        {
            tail.Next = newNode;
            newNode.Prev = tail;
            tail = newNode;
        }
        count++;
    }

    /// <summary>
    /// Удаление первого элемента
    /// </summary>
    public bool RemoveFirst()
    {
        if (IsEmpty)
            return false;

        if (head == tail) // Один элемент
        {
            head = tail = null;
        }
        else
        {
            head = head.Next;
            head.Prev = null;
        }
        count--;
        return true;
    }

    /// <summary>
    /// Удаление последнего элемента
    /// </summary>
    public bool RemoveLast()
    {
        if (IsEmpty)
            return false;

        if (head == tail)
        {
            head = tail = null;
        }
        else
        {
            tail = tail.Prev;
            tail.Next = null;
        }
        count--;
        return true;
    }

    /// <summary>
    /// Удаление элемента по значению (первое вхождение)
    /// </summary>
    public bool Remove(T data)
    {
        LinkedListNode<T> current = head;

        while (current != null)
        {
            if (EqualityComparer<T>.Default.Equals(current.Data, data))
            {
                RemoveNode(current);
                return true;
            }
            current = current.Next;
        }
        return false;
    }

    /// <summary>
    /// Удаление конкретного узла
    /// </summary>
    public void RemoveNode(LinkedListNode<T> node)
    {
        if (node == null)
            throw new ArgumentNullException(nameof(node));

        if (node == head)
        {
            RemoveFirst();
        }
        else if (node == tail)
        {
            RemoveLast();
        }
        else
        {
            node.Prev.Next = node.Next;
            node.Next.Prev = node.Prev;
            count--;
        }
    }

    /// <summary>
    /// Поиск элемента по значению
    /// </summary>
    public LinkedListNode<T> Find(T data)
    {
        LinkedListNode<T> current = head;

        while (current != null)
        {
            if (EqualityComparer<T>.Default.Equals(current.Data, data))
                return current;
            current = current.Next;
        }
        return null;
    }

    /// <summary>
    /// Содержит ли список элемент
    /// </summary>
    public bool Contains(T data)
    {
        return Find(data) != null;
    }

    /// <summary>
    /// Очистка списка
    /// </summary>
    public void Clear()
    {
        head = tail = null;
        count = 0;
        // Сборщик мусора сам освободит память
    }
    // --- ИНТЕРФЕЙСЫ ДЛЯ foreach ---

    public IEnumerator<T> GetEnumerator()
    {
        LinkedListNode<T> current = head;
        while (current != null)
        {
            yield return current.Data;
            current = current.Next;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    // --- ВЫВОД ДЛЯ ОТЛАДКИ ---

    public void PrintForward()
    {
        Console.Write("List (forward):  ");
        foreach (var item in this)
            Console.Write($"[{item}] ");
        Console.WriteLine($"\nCount: {count}");
    }

    public void PrintBackward()
    {
        Console.Write("List (backward): ");
        LinkedListNode<T> current = tail;
        while (current != null)
        {
            Console.Write($"[{current.Data}] ");
            current = current.Prev;
        }
        Console.WriteLine();
    }
}
class Program
{
    static void Main()
    {
    }
}