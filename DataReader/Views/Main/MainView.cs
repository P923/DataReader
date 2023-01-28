using System.Diagnostics;
using Terminal.Gui;

namespace GUI.Views.Main
{
    internal class MainView
    {


        public static MenuBar getMenu(MainController controller)
        {
            MenuBar menu = new(new MenuBarItem[] {
            new MenuBarItem ("_System", new MenuItem [] {
                    new MenuItem ("_Quit", "", () => {
                        Application.RequestStop ();
                        Process.GetCurrentProcess().Kill();
                    }, null, null, Key.Esc)
                }),
            new MenuBarItem ("_Origins", new MenuItem [] {
                new MenuItem ("_Add", "", () => {
                    controller.AddOrigin();
                },null, null, Key.F1),
                new MenuItem ("_Remove", "", () => {
                    controller.RemoveOrigin();
                }, null, null, Key.F2)
                }),
            new MenuBarItem("_Tags", new MenuItem[] {
                new MenuItem ("_Add", "", () => {
                    controller.AddTag();
                },null, null, Key.F5),
                new MenuItem ("_Modify", "", () => {
                    controller.ModifyTag();
                }, null, null, Key.F6),
                new MenuItem ("_Remove", "", () => {
                    controller.RemoveTag();
                }, null, null, Key.F7)
                })
            });
            return menu;
        }

        public static Window getPrincipal()
        {
            return new Window("")
            {
                X = 0,
                Y = 1,
                Width = Dim.Fill(),
                Height = Dim.Fill() - 1
            };

        }

    }
}
