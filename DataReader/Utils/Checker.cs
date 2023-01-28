using NStack;

namespace GUI.Utils
{
    internal class Checker
    {
        public static bool IPChecker(ustring obj)
        {
            string ipString = obj.ToString();
            return !string.IsNullOrWhiteSpace(ipString);
        }
    }
}
