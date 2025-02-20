namespace AssetManagement.Helpers.FileHelper
{
    public class FileManagement
    {
        public void Test()
        {
            FileSystemInfo fileInfo = new FileInfo("");
        }

        static void ShowWindowsDirectoryInfo()
        {
            // Dump directory information. If you are not on Windows, plug in another directory
            DirectoryInfo dir = new DirectoryInfo($@"C{Path.VolumeSeparatorChar}{Path.
            DirectorySeparatorChar}Windows");
            Console.WriteLine("***** Directory Info *****");
            Console.WriteLine("FullName: {0}", dir.FullName);
            Console.WriteLine("Name: {0}", dir.Name);
            Console.WriteLine("Parent: {0}", dir.Parent);
            Console.WriteLine("Creation: {0}", dir.CreationTime);
            Console.WriteLine("Attributes: {0}", dir.Attributes);
            Console.WriteLine("Root: {0}", dir.Root);
            Console.WriteLine("**************************\n");
        }
    }
}