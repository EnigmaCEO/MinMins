#if UNITY_ANDROID || UNITY_IPHONE || UNITY_STANDALONE_OSX || UNITY_TVOS
// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("EmOUFcY5iBQWrnM3nUCymsD4CEE7UX0DXtTVgO11H9Li2TO9OyainsDR+7cDSNTCjwTb1kbnsq4rxM6s/PPwxCgt5kfu54hwbcc8ffpE6Ta7td2k7fOUedw66xaLQs3Z5NXCm/98cn1N/3x3f/98fH3z3lVfqoGWT3HBaaGf84YSjb4jEU0u9qxUYz4hYQU+DL1gblM2QE64n6q18TRYMoVryMw8IOSahQ1zo/16fsQcsiZ2Tf98X01we3RX+zX7inB8fHx4fX6J8eaCTh1igQpc+6FlufqtTxCuJJMw6+gtOXEAOFv+QHW/zpzkwD1z2WRmF8ph//i7wLS9oerTENH6X7Hb3cnRlSFCu5IK6CMVrN8vTq+I5baEHdriuyQzIH9+fH18");
        private static int[] order = new int[] { 12,4,7,7,9,9,13,13,13,12,11,13,12,13,14 };
        private static int key = 125;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
#endif
