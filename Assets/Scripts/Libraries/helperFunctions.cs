using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Seed.Utilities {
    public static class Utils
    {
        // Custom modulo operation dealing with negative indices
        // see: https://stackoverflow.com/questions/1082917/mod-of-negative-number-is-melting-my-brain/1082938
        public static int mod (int k, int n)
        {
            return ((k %= n) < 0) ? k+n : k;
        }
    }
}
