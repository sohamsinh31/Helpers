namespace AssetManagement.Helpers.Extensions
{
    public static class ExtensionHelper
    {
        public static int ToInt(this string str)
        {
            if (!string.IsNullOrEmpty(str))
                return int.Parse(str);
            else
                return -1;
        }
    }
}