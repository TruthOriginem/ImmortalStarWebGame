using UnityEngine;
using System.Collections.Generic;

public class WeightedRandomPicker<T>
{

    private List<T> items = new List<T>();
    private List<float> weights = new List<float>();

    private float total = 0f;
    private bool ignoreWeights;
	
	private Random random = null;

    public WeightedRandomPicker():this(false)
    {
    }

    public WeightedRandomPicker(bool ignoreWeights)
    {
        this.ignoreWeights = ignoreWeights;
    }

    public WeightedRandomPicker(Random random):this(false)
    {
        this.random = random;
    }


    public void clear()
    {
        items.Clear();
        weights.Clear();
        total = 0;
    }

    public void addAll(List<T> items)
    {
        foreach (T item in items)
        {
            Add(item);
        }
    }

    public void Add(T item)
    {
        Add(item, 1f);
    }
    public void Add(T item, float weight)
    {
        //if (weight < 0) weight = 0;
        if (weight <= 0) return;
        items.Add(item);
        weights.Add(weight); // + (weights.isEmpty() ? 0 : weights.get(weights.size() - 1)));
        total += weight;
    }

    public void Remove(T item)
    {
        int index = items.IndexOf(item);
        if (index != -1)
        {
            items.RemoveAt(index);
            float weight = weights[index];
            weights.RemoveAt(index);
            total -= weight;
        }
    }

    public bool IsEmpty()
    {
        return items.Count == 0;
    }

    public List<T> GetItems()
    {
        return items;
    }

    public float GetWeight(int index)
    {
        return weights[index];
    }

    public T PickAndRemove()
    {
        T picka = Pick();
        Remove(picka);
        return picka;
    }

    public T Pick()
    {
        if (items.Count == 0) return default(T);

        if (ignoreWeights)
        {
            return items[(int)(Random.value * items.Count)];
        }

        float random;
        if (this.random != null)
        {
            random = Random.value * total;
        }
        else
        {
            random = (float)(Random.value * total);
        }
        float weightSoFar = 0f;
        int index = 0;
        foreach (float weight in weights)
        {
            weightSoFar += weight;
            if (random <= weightSoFar) break;
            index++;
        }
        return items[Mathf.Min(index, items.Count - 1)];
    }

    public Random GetRandom()
    {
        return random;
    }

    public void SetRandom(Random random)
    {
        this.random = random;
    }
}
