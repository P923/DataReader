using GUI.Data;
using GUI.Utils;
using NStack;
using Terminal.Gui;

namespace GUI.Views.Main
{
    internal class TagView : Observable
    {
        public List<ustring> items;
        public Window view;
        private ListView list;

        public TagView(List<DataTag> items) : base()
        {
            this.items = new List<ustring>();
            UpdateList(items);
        }

        public Window Render(View plcWin)
        {
            view = new Window("Tag")
            {
                X = Pos.Right(plcWin),
                Y = 0,
                Width = Dim.Percent(50),
                Height = Dim.Percent(80)
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
            return view;
        }

        private void UpdateLabel(ListViewItemEventArgs obj)
        {
            view.Title = "Tag" + " (Selected:" + list.SelectedItem.ToString() + ")";
        }

        private void ReturnToMain(object _)
        {
            SendToSubscriber(Constants.SELECTED_INDEX_TAG, list.SelectedItem);
        }
        public void UpdateList(IEnumerable<DataTag> tags)
        {
            items.Clear();
            foreach (DataTag tag in tags)
            {
                ustring name = (ustring)tag.ToString();
                items.Add(name);
            }
        }

        public int GetSelectedIndex()
        {
            return list.SelectedItem;
        }

    }
}
