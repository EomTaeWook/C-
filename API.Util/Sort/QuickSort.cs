using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Util.Sort
{
    class QuickSort
    {
        public static List<T> Sort<T>(List<T> unsortList) where T : IComparable
        {
            Sort(ref unsortList, 0, unsortList.Count - 1);

            return unsortList;
        }
        private static void Swap<T>(ref List<T> unsortList, int indexA, int indexB)
        {
            var temp = unsortList[indexA];
            unsortList[indexA] = unsortList[indexB];
            unsortList[indexB] = temp;
        }
        private static List<T> Sort<T>(ref List<T> unsortList, int left, int right) where T : IComparable
        {
            var pivot = unsortList[(left + right) / 2];
            int l = left, r = right;
            while (l <= r)
            {
                while (unsortList[l].CompareTo(pivot) < 0)
                {
                    l++;
                }
                while (unsortList[r].CompareTo(pivot) > 0)
                {
                    r--;
                }
                if (l <= r)
                {
                    Swap(ref unsortList, l, r);
                    l++;
                    r--;
                }
            }
            if (left < r)
                Sort(ref unsortList, left, r);
            if (l < right)
                Sort(ref unsortList, l, right);

            return unsortList;
        }
    }
}
