using System;

namespace SteamMatchmaking.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Checks the type of the object against the type of object passed in. Returns true if they match.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="newObj"></param>
        /// <returns></returns>
        public static bool IsTypeCode<T>(this IConvertible obj, T newObj)
        {
            try
            {
                Convert.ChangeType(obj, (TypeCode)Convert.ToInt32(newObj));
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Inferred conversion to type specified
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T To<T>(this IConvertible obj)
        {
            return (T)Convert.ChangeType(obj, typeof(T));
        }

        public static T ToOrDefault<T>(this IConvertible obj)
        {
            try
            {
                return To<T>(obj);
            }
            catch
            {
                return default(T);
            }
        }

        public static bool ToOrDefault<T>(this IConvertible obj, out T newObj)
        {
            try
            {
                newObj = To<T>(obj);
                return true;
            }
            catch
            {
                newObj = default(T);
                return false;
            }
        }

        public static T ToOrOther<T>(this IConvertible obj, T other)
        {
            try
            {
                return To<T>(obj);
            }
            catch
            {
                return other;
            }
        }

        public static bool ToOrOther<T>(this IConvertible obj, out T newObj,
                                        T other)
        {
            try
            {
                newObj = To<T>(obj);
                return true;
            }
            catch
            {
                newObj = other;
                return false;
            }
        }

        public static T ToOrNull<T>(this IConvertible obj) where T : class
        {
            try
            {
                return To<T>(obj);
            }
            catch
            {
                return null;
            }
        }

        public static bool ToOrNull<T>(this IConvertible obj, out T newObj) where T : class
        {
            try
            {
                newObj = To<T>(obj);
                return true;
            }
            catch
            {
                newObj = null;
                return false;
            }
        }
    }
}
