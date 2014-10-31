using System;
using System.Collections.Generic;
using System.Text;
using LTreesLibrary.Trees.Instructions;

namespace LTreesLibrary.Trees
{
    /// <summary>
    /// Maps keys to lists of values.
    /// </summary>
    /// <remarks>
    /// Not designed for general-purpose use.
    /// </remarks>
    class MultiMap<KeyType, ValueType>
    {
        private Dictionary<KeyType, List<ValueType>> map = new Dictionary<KeyType, List<ValueType>>();

        public Dictionary<KeyType, List<ValueType>> Map
        {
            get { return map; }
        }

        public List<ValueType> Values
        {
            get
            {
                List<ValueType> list = new List<ValueType>();
                foreach (List<ValueType> sub in map.Values)
                {
                    list.AddRange(sub);
                }
                return list;
            }
        }

        public List<ValueType> this[KeyType index]
        {
            get { return map[index]; }
        }

        public Boolean ContainsKey(KeyType key)
        {
            return map.ContainsKey(key);
        }
	

        public void Add(KeyType name, ValueType production)
        {
            List<ValueType> list;
            if (map.ContainsKey(name))
            {
                list = map[name];
            }
            else
            {
                list = new List<ValueType>();
                map[name] = list;
            }
            list.Add(production);
        }
    }
}
