using System.Collections.Generic;

namespace ENet.Managed.Internal
{
    internal static class InternalExtensionMethods
    {
        public static bool OrderlessRemove<T>(this List<T> list, T item)
        {
            var index = list.FindIndex(p => p!.Equals(item));
            if (index < 0) return false;
            list.OrderlessRemoveAt(index);
            return true;
        }

        public static void OrderlessRemoveAt<T>(this List<T> list, int index)
        {
            if (list.Count > 2 && list.Count - 1 != index)
            {
                var lastIndex = list.Count - 1;
                var last = list[lastIndex];
                list[index] = last;
                list.RemoveAt(lastIndex);
            }
            else
            {
                list.RemoveAt(index);
            }
        }

#if NET46 || NETSTANDARD2_0 // Queue<T>.TryDequeue() is missing in these frameworks
        public static bool TryDequeue<T>(this Queue<T> queue, out T val)
        {
            if (queue.Count == 0)
            {
                val = default!;
                return false;
            }
            else
            {
                val = queue.Dequeue();
                return true;
            }
        }
#endif
    }
}
