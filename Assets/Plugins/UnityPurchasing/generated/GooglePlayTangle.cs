#if UNITY_ANDROID || UNITY_IPHONE || UNITY_STANDALONE_OSX || UNITY_TVOS
// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("Q0CUlUSg5iAeg5JTwSNECmV8B53Am1u4ax/B73emN68dDF5JJBxcNIe4EcCnwll7OeEZ0i2rRHawtwYEOQk1Xu7kJA9MXtyfvZVyNUZyXS81hwQnNQgDDC+DTYPyCAQEBAAFBkjnxPBfyDYIcm7zRIh8sWGTV7TZhjoNpL3vBoRBOUnC6q3L6A24JzrME/CfjSwFmseNc0tk5vUUUGbLLA2DPWc0XgoRFJRyK4AYje8SiguXoqCu4aM0wxsQ5x+vcd8LKXQqUlDeAuuaHZTrHa07SY+O9NOjqiP+wBMHG0oQKh+NaeZ87yDUaxZ/RYF9hwQKBTWHBA8HhwQEBZjR9pxUqAQTTFfJJCui+S6LxdZRgS1re5Zumcsh3ucUW8YTagcGBAUE");
        private static int[] order = new int[] { 5,4,10,8,5,9,6,7,11,11,12,12,12,13,14 };
        private static int key = 5;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
#endif
