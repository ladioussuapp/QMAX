using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Xml.Serialization;

namespace Data
{
    [System.Serializable]
    public class BattleSysData : ScriptableObject
    {
        public int TestInt = 0;
        public Dictionary<string, int> TestDict;
        public TestData TestData;
        public ScriptableDictionary TestScriptableDict;
    }

    [System.Serializable]
    public class TestData
    {

        public string key;
        public int val;
    }

    [System.Serializable]
    public class ScriptableDictionary : IDictionary<string, object>
    {
        [System.Serializable]
        public class KeyCollection : ICollection<string>
        {
            [SerializeField]
            public List<string> items = new List<string>();

            public void Add(string item)
            {
                items.Add(item);
            }

            public void Clear()
            {
                items = new List<string>();
            }

            public bool Contains(string item)
            {
                return items.IndexOf(item) > -1;
            }

            public void Copystringo(string[] array, int arrayIndex)
            {
                throw new System.NotImplementedException();
            }

            public int Count
            {
                get { return items.Count; }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public bool Remove(string item)
            {
                return items.Remove(item);
            }

            public IEnumerator GetEnumerator()
            {
                throw new System.NotImplementedException();
            }

            IEnumerator<string> IEnumerable<string>.GetEnumerator()
            {
                throw new System.NotImplementedException();
            }

            public void CopyTo(string[] array, int arrayIndex)
            {
                throw new System.NotImplementedException();
            }
        }

        [System.Serializable]
        public class ValCollection : ICollection<object>
        {
            public List<string> stringItems = new List<string>();
            [System.NonSerialized] 
            public List<ScriptableDictionary> dictItems = new List<ScriptableDictionary>();

            public object this[int key]
            {
                get
                {
                    if (dictItems[key] == null)
                    {
                        return stringItems[key];
                    }
                    else
                    {
                        return dictItems[key];
                    }
                }
                set
                {
                    if (value is ScriptableDictionary)
                    {
                        dictItems[key] = value as ScriptableDictionary;
                    }
                    else
                    {
                        stringItems[key] = value.ToString();
                    }
                }
            }

            public void Add(object item)
            {
                if (item is ScriptableDictionary)
                {
                    dictItems.Add(item as ScriptableDictionary);
                    stringItems.Add(null);
                }
                else
                {
                    dictItems.Add(null);
                    stringItems.Add(item.ToString());
                }
            }

            public void Clear()
            {
                stringItems = new List<string>();
                dictItems = new List<ScriptableDictionary>();
            }

            public bool Contains(object item)
            {
                if (item is ScriptableDictionary)
                {
                    return dictItems.IndexOf(item as ScriptableDictionary) > -1;
                }
                else
                {
                    return stringItems.IndexOf(item.ToString()) > -1;
                }
            }

            public void Copystringo(object[] array, int arrayIndex)
            {
                throw new System.NotImplementedException();
            }

            public int Count
            {
                get { return stringItems.Count; }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public bool Remove(object item)
            {
                if (item is ScriptableDictionary)
                {
                    return dictItems.Remove(item as ScriptableDictionary);
                }
                else
                {
                    return stringItems.Remove(item.ToString());
                }
            }

            public IEnumerator GetEnumerator()
            {
                throw new System.NotImplementedException();
            }

            IEnumerator<object> IEnumerable<object>.GetEnumerator()
            {
                throw new System.NotImplementedException();
            }

            public void CopyTo(object[] array, int arrayIndex)
            {
                throw new System.NotImplementedException();
            }
        }

        public KeyCollection keys = new KeyCollection();
        public ValCollection vals = new ValCollection();


        public void Add(string key, object value)
        {
            keys.Add(key);

            vals.Add(value);
        }

        public bool ContainsKey(string key)
        {
            return keys.Contains(key);
        }

        ICollection<string> IDictionary<string, object>.Keys
        {
            get { return keys; }
        }

        public bool Remove(string key)
        {
            return keys.Remove(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            if (keys.Contains(key))
            {
                value = this[key];
            }

            value = null;
            return value != null;
        }

        public ICollection<object> Values
        {
            get { return vals; }
        }

        public object this[string key]
        {
            get
            {
                for (int i = 0; i < keys.Count; i++)
                {
                    if (keys.items[i] == key)
                    {
                        return vals[i];
                    }
                }

                return null;
            }
            set
            {
                for (int i = 0; i < keys.Count; i++)
                {
                    if (keys.items[i] == key)
                    {
                        if (value is string)
                        {
                            vals[i] = value;
                        }
                    }
                }
            }
        }

        public IEnumerator GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        public void Add(KeyValuePair<string, object> item)
        {
            throw new System.NotImplementedException();
        }

        public void Clear()
        {
            keys = new KeyCollection();
            vals = new ValCollection();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            int index = keys.items.IndexOf(item.Key);

            if (index > -1 && vals[index] == item.Value)
            {
                return true;
            }

            return false;
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new System.NotImplementedException();
        }

        public int Count
        {
            get { return keys.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            throw new System.NotImplementedException();
        }

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            throw new System.NotImplementedException();
        }
    }
}