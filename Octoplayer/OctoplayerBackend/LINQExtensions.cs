using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace OctoplayerBackend
{
    public static class LINQExtensions
    {
        private static Random rng = new Random();

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            var list = source.ToList();
            for(var i = list.Count() - 1; i >= 0; i--)
            {
                int r = rng.Next(i);
                yield return list[r];
                list[r] = list[i];
            }
        }
    }
}
