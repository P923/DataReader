using GUI.Data;
using GUI.Utils;
using Terminal.Gui;

namespace GUI.Views.Main
{
    internal class LiveView : EventSubscriber
    {
        public static SessionTracker sessions;
        private OriginRequest reader;
        public Window view;
        public Label label;
        public Label value;


        public LiveView()
        {
            sessions = new SessionTracker();
            reader = new OriginRequest(sessions);
        }

        public Window Render(Window plcWin, Window tagWin)
        {
            Window? w = new("Live Read")
            {
                X = Pos.Right(plcWin),
                Y = Pos.Bottom(tagWin),
                Width = Dim.Percent(50),
                Height = Dim.Percent(25)
            };

            label = new Label();
            value = new Label() { Y = Pos.Bottom(label) };
            w.Add(label);
            w.Add(value);
            reader.AddSubscriber(this);
            return w;
        }

        internal void UpdateLive(DataTag dataTag)
        {
            if (dataTag != null)
            {
                reader.ReadOriginTagAsync(dataTag);
            }
            else
            {
                label.Text = "";
            }
        }

        public void Result(int from, object result)
        {
            label.Text = (string)result;
        }
    }
}
