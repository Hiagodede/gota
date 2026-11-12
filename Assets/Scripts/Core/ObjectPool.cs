using System.Collections.Generic;
using UnityEngine;

namespace Gota
{
    /// <summary>
    /// Pool genérico: cria todos os objetos no início (prewarm) e depois só
    /// liga/desliga. Nenhum Instantiate/Destroy durante a partida.
    /// </summary>
    public class ObjectPool<T> where T : Component
    {
        private readonly T prefab;
        private readonly Transform parent;
        private readonly Stack<T> available = new Stack<T>();
        private readonly List<T> all = new List<T>();

        public int CountAll => all.Count;
        public int CountAvailable => available.Count;

        public ObjectPool(T prefab, Transform parent, int prewarm)
        {
            this.prefab = prefab;
            this.parent = parent;
            for (int i = 0; i < prewarm; i++)
            {
                available.Push(CreateNew());
            }
        }

        public T Get()
        {
            T item = available.Count > 0 ? available.Pop() : CreateNew();
            item.gameObject.SetActive(true);
            return item;
        }

        public void Return(T item)
        {
            if (item == null) return;
            item.gameObject.SetActive(false);
            item.transform.SetParent(parent, false);
            available.Push(item);
        }

        /// <summary>Devolve ao pool tudo que estiver ativo.</summary>
        public void ReturnAll()
        {
            foreach (var item in all)
            {
                if (item != null && item.gameObject.activeSelf)
                {
                    Return(item);
                }
            }
        }

        private T CreateNew()
        {
            T item = Object.Instantiate(prefab, parent);
            item.gameObject.SetActive(false);
            all.Add(item);
            return item;
        }
    }
}
