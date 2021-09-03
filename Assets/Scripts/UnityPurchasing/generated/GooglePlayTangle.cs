// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("QFRIGUN5TN46tS+8c4c4RSwW0i6TyAjrOEySvCT1ZPxOXw0ad08PZ59Ao8zef1bJlN4gGDe1pkcDNZh/1OtCk/SRCihqskqBfvgXJePkVVdqWmYNvbd3XB8Nj8zuxiFmFSEOfI1RuMlOx7hO/mga3N2ngPD5cK2TXtBuNGcNWUJHxyF400vevEHZWMRm1Fd0ZltQX3zQHtChW1dXV1NWVdVpXvfuvFXXEmoakbn+mLte63RpQB8Emnd48ap92JaFAtJ+OCjFPcrx8/2y8GeQSEO0TPwijFh6J3kBAxu0l6MMm2VbIT2gF9sv4jLABOeKEBPHxhfztXNN0MEAknAXWTYvVM7UV1lWZtRXXFTUV1dWy4Klzwf7V5hyjbRHCJVAOVRVV1ZX");
        private static int[] order = new int[] { 2,4,7,10,8,12,11,7,11,13,10,13,12,13,14 };
        private static int key = 86;

        public static readonly bool IsPopulated = true;


        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
