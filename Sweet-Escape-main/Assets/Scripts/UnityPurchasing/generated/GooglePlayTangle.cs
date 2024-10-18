// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("SsnHyPhKycLKSsnJyEXqvWJsPkFZWXND2yHP9NsgsTtdUBvr4cOK1YUZbLwn4lwNZRAO+jZWezfqo9KhrIP6Dcb8ztYj5+zeMQrisgRiDObukRRRZxC2vvgYV0PsFzM52wZM3ASh/ry3VoV04UZBlyuKYSAxHOrdmjYZpXX5zP3nXX6xeU48twNd618JmWd2fvoxQrHvHIH/lhTmBwvQnEyuGBA7PL1S4u9lRemTve2bMBRMhgcJ/lB0a1XZywmGyTkrmbQLfa714qZ+qhLUV1bUOiyA2JTJH0a9FQ/lUKJ4lxbecNYBifoc+HAzly7V+ErJ6vjFzsHiToBOP8XJycnNyMvYA8Y+WCuiy0I0aDuVZYG9wkpA12Zvd47WQCptucrLycjJ");
        private static int[] order = new int[] { 1,11,13,9,4,6,7,13,8,13,12,13,13,13,14 };
        private static int key = 200;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
