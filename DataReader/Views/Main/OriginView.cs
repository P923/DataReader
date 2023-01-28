using GUI.Data;
using GUI.Utils;
using NStack;
using Terminal.Gui;

namespace GUI.Views.Main
{
    internal class OriginView : Observable
    {
        public List<ustring> items;
        public Window view;
        private ListView list;

        public OriginView(List<DataOrigin> items) : base()
        {
            this.items = new List<ustring>();
            UpdateList(items);
        }

        public Window Render()
        {
            view = new Window("Origin")
            {
                X = 0,
                Y = Pos.Center(),
                Width = Dim.Percent(50),
                Height = Dim.Fill()
            };

            list = new ListView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill() - 5,
                Height = Dim.Fill()
            };

            list.SelectedItemChanged += UpdateLabel;
            list.SelectedItemChanged += ReturnToMain;


            list.SetSource(items);
            view.Add(list);
            UpdateLabel(null);
            ReturnToMain(null);
            return view;
        }


        private void ReturnToMain(object _)
        {
            SendToSubscriber(Constants.SELECTED_INDEX_ORIGIN, list.SelectedItem);
        }

        private void UpdateLabel(ListViewItemEventArgs _)
        {
            view.Title = "Origin" + " (Selected:" + list.SelectedItem.ToString() + ")";
        }

        public void UpdateList(List<DataOrigin> origins)
        {
            items.Clear();
            foreach (DataOrigin origin in origins)
            {
                ustring name = (ustring)origin.Name;
                items.Add(name);
            }

            if (list != null)
            {
                list.SelectedItem = 0;
                ReturnToMain(null);
            }
        }

        public int GetSelectedIndex()
        {
            return list.SelectedItem;
        }

    }
}
