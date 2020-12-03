using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Burrows_WhellerTransform
{
    public sealed class ListComparer<T> : IComparer<IReadOnlyList<T>> where T : IComparable
    {
        public int Compare(IReadOnlyList<T> left, IReadOnlyList<T> right)
        {
            if (left is null) return right is null ? 0 : -1;
            if (right is null) return 1;

            var innerComparer = Comparer<T>.Default;
            int count = Math.Min(left.Count, right.Count);
            for (int index = 0; index < count; index++)
            {
                int result = innerComparer.Compare(left[index], right[index]);
                if (result != 0) return result;
            }

            return left.Count.CompareTo(right.Count);
        }
    }
}
