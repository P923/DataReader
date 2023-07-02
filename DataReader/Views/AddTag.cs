using GUI.Data;
using GUI.Utils;
using GUI.Views.Main;
using NStack;
using System.Text.RegularExpressions;
using Terminal.Gui;

namespace GUI.Views
{
    internal class AddTag : Observable, EventSubscriber
    {
        private readonly DataOrigin origin;
        private readonly View principalWin;
        private Button addButton;
        private TextField txtTag;
        private TextField txtName;
        private ComboBox comboType;
        private Label lblError;
        private Label lblPreview;
        private TextField txtScan;
        private Dialog dialog;
        private OriginRequest originRequest;
        private DataTag tag;
        private TextField txtDefault;

        public AddTag(DataOrigin origin) : base()
        {
            this.origin = origin;
        }

        public AddTag(DataOrigin origin, DataTag tag) : base()
        {
            this.origin = origin;
            this.tag = tag;
        }

        public View render()
        {
            dialog = new Dialog("Add Tag");
            Label? lblName = new("Name:       ")
            {
                X = Pos.Center() - 20
            };

            txtName = new TextField("")
            {
                X = Pos.Right(lblName),
                Y = lblName.Y,
                Width = 40,
                Text = "Tag"
            };
            txtName.TextChanged += CheckCorrect;

            ////////////////////////////////////////////////////
            Label? lblAddress = new("Address:    ")
            {
                Y = Pos.Bottom(lblName),
                X = Pos.Center() - 20
            };

            txtTag = new TextField("")
            {
                X = Pos.Right(lblAddress),
                Y = lblAddress.Y,
                Width = 40
            };
            txtTag.TextChanged += CheckCorrect;

            //////////////////////////////////////////////////
            Label? lblScan = new("Scan Class: ")
            {
                X = Pos.Center() - 20,
                Y = Pos.Bottom(lblAddress)
            };
            txtScan = new TextField("100")
            {
                X = Pos.Right(lblScan),
                Y = lblScan.Y,
                Width = 40
            };
            txtScan.TextChanged += CheckCorrect;
            //////////////////////////////////////////////////
            Label? lblDefault = new("Default:    ")
            {
                X = Pos.Center() - 20,
                Y = Pos.Bottom(lblScan)
            };
            txtDefault = new TextField("0")
            {
                X = Pos.Right(lblDefault),
                Y = lblDefault.Y,
                Width = 40
            };
            txtDefault.TextChanged += CheckCorrect;

            /////////////////////////////////////////////////
            List<string> items = Filler.TypeTag();
            comboType = new ComboBox()
            {
                X = Pos.Center() - 10,
                Y = Pos.Bottom(txtScan) + 2,
                Width = 40
            };
            comboType.SetSource(items);
            comboType.SelectedItem = 0;
            comboType.SelectedItemChanged += CheckCorrect2;
            //////////////////////////////////////////////////
            addButton = new Button("Add")
            {
                X = Pos.Center() - 20,
                Y = Pos.Bottom(comboType) + 2,
            };
            addButton.Clicked += Add_Clicked;

            Button? exitButton = new("Exit")
            {
                X = Pos.Center(),
                Y = Pos.Bottom(comboType) + 2,
            };
            exitButton.Clicked += Exit_Clicked;
            ////////////////////////////////////////////////
            lblError = new Label("")
            {
                X = Pos.Center(),
                Y = Pos.Bottom(addButton) + 1,
            };

            lblPreview = new Label("")
            {
                X = 0,
                Y = Pos.Bottom(lblError) + 1,
                Width = 20
            };


            dialog.Add(lblName, txtName);
            dialog.Add(lblAddress, txtTag);
            dialog.Add(lblScan, txtScan);
            dialog.Add(lblDefault, txtDefault);
            dialog.Add(comboType);
            dialog.Add(addButton, exitButton);
            dialog.Add(lblError);
            dialog.Add(lblPreview);

            originRequest = new OriginRequest(LiveView.sessions);
            originRequest.AddSubscriber(this);

            if (tag != null)
            {
                string defaultV = tag.DefaultValue;
                int scanClass = tag.ScanClass;
                string address = tag.Address;
                int index = Filler.TypeTag().IndexOf(tag.Type);
                txtName.Text = tag.Name;
                txtTag.Text = address;
                txtScan.Text = scanClass.ToString();
                txtDefault.Text = defaultV;
                comboType.SelectedItem = index;
            }



            CheckCorrect(null);
            return dialog;
        }

        private void CheckCorrect2(ListViewItemEventArgs obj)
        {
            CheckCorrect(null);
        }

        private void CheckCorrect(ustring obj)
        {
            string name = txtName.Text.ToString().Trim();
            string tag = txtTag.Text.ToString().Trim();
            string scan = txtScan.Text.ToString().Trim();
            string defaultv = txtDefault.Text.ToString().Trim();
            bool enabled = true;

            if (name.Length == 0)
            {
                enabled = false;
                lblError.Text = "Insert a name.";
            }

            if (tag.Length == 0)
            {
                enabled = false;
                lblError.Text = "Insert a tag address.";
            }

            if (scan.Length == 0)
            {
                enabled = false;
                lblError.Text = "Insert a scan rate.";
            }

            if (scan.Length == 0)
            {
                enabled = false;
                lblError.Text = "Insert a scan rate.";
            }

            Regex r = new(@"[a-zA-Z]+");
            if (r.IsMatch(defaultv))
            {
                enabled = false;
                lblError.Text = "Insert a valid default value.";
            }

            if (enabled)
            {
                lblError.Text = "";

            }


            originRequest.ReadOriginTagAsync(getDataTag());
            addButton.Enabled = enabled;

        }


        private void Add_Clicked()
        {
            DataTag data = getDataTag();
            SendToSubscriber(Constants.ADD_DATA_TAG, data);
            SendToSubscriber(Constants.CLOSE, dialog);

        }

        private DataTag getDataTag()
        {
            if (tag == null)
            {
                tag = new DataTag();
            }
            int scan = 100;
            if (txtScan.Text.ToString().Trim().Length > 1)
            {
                scan = int.Parse(txtScan.Text.ToString().Trim());
            }

            tag.DataOrigin = origin;
            tag.Address = txtTag.Text.ToString().Trim();
            tag.Type = comboType.Text.ToString().Trim();
            tag.ScanClass = scan;
            tag.Name = txtName.Text.ToString().Trim();
            tag.DefaultValue = txtDefault.Text.ToString().Trim();
            return tag;
        }

        private void Exit_Clicked()
        {
            SendToSubscriber(Constants.EXIT, dialog);
        }

        public void Result(int from, object result)
        {
            lblPreview.Text = (string)result;
        }
    }
}
