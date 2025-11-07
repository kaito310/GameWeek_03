using System.Collections.Generic;

/// <summary>
/// 優先度付きキューを実装
/// </summary>
/// <typeparam name="T"></typeparam>
public class PriorityQueue<T>
{
    List<(T item, int priority)> elements = new();

    public int Count => elements.Count;

    /// <summary>
    /// 新たな要素を入れる
    /// </summary>
    /// <param name="item">要素</param>
    /// <param name="priority">優先度を比較するための値</param>
    public void Enqueue(T _item, int _priority)
    {
        elements.Add((_item, _priority));
    }

    /// <summary>
    /// 優先度が最も高い要素を取り出す
    /// (優先度は、priorityの値が小さい程高くなる)
    /// </summary>
    /// <returns>配列内で、最も優先度が高い要素</returns>
    public T Dequeue()
    {
        int bestIdx = 0;
        for (int i = 1; i < elements.Count; i++)
        {
            if (elements[i].priority < elements[bestIdx].priority)
            {
                bestIdx = i;
            }
        }

        // 比較の結果、最も優先度が高い要素を取り出す
        T bestItem = elements[bestIdx].item;
        elements.RemoveAt(bestIdx);
        return bestItem;
    }
}
