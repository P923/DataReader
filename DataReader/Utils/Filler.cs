using Terminal.Gui;

namespace GUI.Utils
{
    public class Filler
    {
        private static readonly Random random = new();
        public static List<string> TypeOrigin()
        {
            List<string> list = new()
            {
                Constants.AB,
                Constants.MODBUS,
                Constants.XML,
                Constants.OPCServer
            };
            return list;
        }

        public static List<string> TypeTag()
        {
            List<string> list = new()
            {
                Constants.BOOLEAN,
                Constants.SHORT,
                Constants.INTEGER,
                Constants.LONG,
                Constants.FLOAT,
                Constants.DOUBLE
            };
            return list;
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        internal static void NoOrigin()
        {
            _ = MessageBox.ErrorQuery(50, 7,
                        "ERROR", "No Data Origins Present!", "OK");
        }

        internal static void NoTag()
        {
            _ = MessageBox.ErrorQuery(50, 7,
                        "ERROR", "No Tags Present!", "OK");
        }


    }


}
