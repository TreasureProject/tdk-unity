using UnityEngine;
using System.Collections.Generic;

namespace Treasure
{
    public static class TDKServiceLocator
    {
        static Dictionary<object, object> services = null;

        public static T GetService<T>(bool createObjIfNotFound = true) where T : Object
        {
            if(services == null)
            {
                services = new Dictionary<object, object>();
            }

            try
            {
                if (services.ContainsKey(typeof(T)))
                {
                    T service = (T)services[typeof(T)];
                    if (service != null)
                    {
                        return service;
                    }
                    else
                    {
                        services.Remove(typeof(T));
                        return FindService<T>(createObjIfNotFound);
                    }
                }
                else
                {
                    return FindService<T>(createObjIfNotFound);
                }
            }
            catch (System.Exception e)
            {
                throw new System.NotImplementedException("Can't find requested service, and create new one is set to " + createObjIfNotFound + ":" + e);
            }
        }

        static T FindService<T>(bool createObjIfNotFound = true) where T : Object
        {
            T type = GameObject.FindObjectOfType<T>();
            if (type != null)
            {
                services.Add(typeof(T), type);
            }
            else if (createObjIfNotFound)
            {
                GameObject go = new GameObject(typeof(T).Name, typeof(T));
                services.Add(typeof(T), go.GetComponent<T>());
            }
            return (T)services[typeof(T)];
        }
    }
}