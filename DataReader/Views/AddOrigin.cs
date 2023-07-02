using GUI.Data;
using GUI.Utils;
using NStack;
using Terminal.Gui;

namespace GUI.Views
{
    internal class AddOrigin : Observable
    {
        private readonly View principalWin;
        private Button addButton;
        private TextField txtIP;
        private TextField txtName;
        private ComboBox comboPLC;
        private Label lblError;
        private TextField txtPath;
        private Dialog dialog;

        public AddOrigin() : base()
        {
        }

        public View render()
        {
            dialog = new Dialog("Add Data Origin");
            Label? lblName = new("Name:       ")
            {
                X = Pos.Center() - 20
            };

            txtName = new TextField("")
            {
                X = Pos.Right(lblName),
                Y = lblName.Y,
                Width = 40,
                Text = "PLC"
            };
            txtName.TextChanged += CheckCorrect;

            ////////////////////////////////////////////////////
            Label? lblIP = new("Gateway IP: ")
            {
                Y = Pos.Bottom(lblName),
                X = Pos.Center() - 20
            };

            txtIP = new TextField("")
            {
                X = Pos.Right(lblIP),
                Y = lblIP.Y,
                Width = 40
            };
            txtIP.TextChanged += CheckCorrect;

            //////////////////////////////////////////////////
            Label? lblPath = new("Path:       ")
            {
                X = Pos.Center() - 20,
                Y = Pos.Bottom(lblIP)
            };
            txtPath = new TextField("")
            {
                X = Pos.Right(lblPath),
                Y = lblPath.Y,
                Width = 40
            };
            /////////////////////////////////////////////////
            List<string> items = Filler.TypeOrigin();
            comboPLC = new ComboBox()
            {
                X = Pos.Center() - 10,
                Y = Pos.Bottom(txtPath) + 1,
                Width = 40
            };
            comboPLC.SetSource(items);
            comboPLC.SelectedItem = 0;
            //////////////////////////////////////////////////
            addButton = new Button("Add")
            {
                X = Pos.Center() - 20,
                Y = Pos.Bottom(comboPLC) + 2,
                Enabled = false
            };
            addButton.Clicked += Add_Clicked;

            Button? exitButton = new("Exit")
            {
                X = Pos.Center(),
                Y = Pos.Bottom(comboPLC) + 2,
            };
            exitButton.Clicked += Exit_Clicked;
            ////////////////////////////////////////////////
            lblError = new Label("")
            {
                X = Pos.Center(),
                Y = Pos.Bottom(addButton) + 1,
            };

            dialog.Add(lblName, txtName);
            dialog.Add(lblIP, txtIP);
            dialog.Add(lblPath, txtPath);
            dialog.Add(comboPLC);
            dialog.Add(addButton, exitButton);
            dialog.Add(lblError);

            CheckCorrect(null);
            return dialog;
        }

        private void CheckCorrect(ustring obj)
        {
            string comboTXT = comboPLC.Text.ToString().Trim();
            string name = txtName.Text.ToString().Trim();
            bool enabled = true;

            if (!Checker.IPChecker(txtIP.Text))
            {
                enabled = false;
                lblError.Text = "Error in IP.";
            }

            if (name.Length == 0)
            {
                enabled = false;
                lblError.Text = "Insert a name.";
            }

            if (!Filler.TypeOrigin().Contains(comboTXT))
            {
                enabled = false;
                lblError.Text = "Insert a correct type of Data Origin.";
            }

            if (comboTXT.Equals(Constants.OPCServer))
            {
                lblError.Text += "\n" +
                    "You can insert Username:Password in Path.";
            }

            if (enabled)
            {
                lblError.Text = "";
            }
            addButton.Enabled = enabled;

        }

        private void Add_Clicked()
        {
            DataOrigin data = new(
                txtName.Text.ToString().Trim(),
                txtIP.Text.ToString().Trim(),
                txtPath.Text.ToString().Trim(),
                comboPLC.Text.ToString().Trim());

            SendToSubscriber(Constants.ADD_DATA_ORIGIN, data);
            SendToSubscriber(Constants.CLOSE, dialog);

        }

        private void Exit_Clicked()
        {
            SendToSubscriber(Constants.EXIT, dialog);
        }
    }
}
