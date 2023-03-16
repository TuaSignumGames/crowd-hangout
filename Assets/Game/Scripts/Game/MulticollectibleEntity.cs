using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MulticollectibleEntity<T> where T : MonoBehaviour
{
    protected T entity;

    protected MulticollectibleElement element;

    public T Entity => entity;

    public MulticollectibleElement Element => element;

    public Transform Transform => Entity.transform;

    public bool IsCollecting => element.IsActive && !element.IsCollected;

    public MulticollectibleEntity(T entity, MulticollectibleElement element)
    {
        this.entity = entity;
        this.element = element;
    }

    public void Collect()
    {
        element.Collect();

        entity.gameObject.SetActive(true);
    }
}
