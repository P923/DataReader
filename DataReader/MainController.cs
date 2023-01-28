using GUI.Data;
using GUI.Utils;
using GUI.Views;
using GUI.Views.Main;
using Microsoft.EntityFrameworkCore;
using Terminal.Gui;

namespace GUI
{
    internal class MainController : EventSubscriber
    {
        private SQLConnector dbOrigin;
        private static MenuBar menu;
        private static Window principalWin;
        private OriginView originView;
        private TagView tagView;
        private LiveView liveView;

        private DbSet<DataOrigin> origins;
        private DbSet<DataTag> tags;
        private bool dialogOpen;

        public void Start()
        {
            Application.Init();
            dbOrigin = new SQLConnector();

            // Menu and Principal Layout
            menu = MainView.getMenu(this);
            principalWin = MainView.getPrincipal();

            // Origin Class
            origins = dbOrigin.DataOrigins;
            originView = new OriginView(origins.ToList());
            Window? originWin = originView.Render();
            originView.AddSubscriber(this);

            // Tag Class
            tags = dbOrigin.Tags;
            tagView = new TagView(new List<DataTag>());
            Window? tagWin = tagView.Render(originWin);
            tagView.AddSubscriber(this);

            // Live Read
            liveView = new LiveView();
            Window? liveRead = liveView.Render(originWin, tagWin);
            liveRead.CanFocus = false;

            principalWin.Add(originWin);
            principalWin.Add(tagWin);
            principalWin.Add(liveRead);
            principalWin.Add(menu);

            Application.Top.Add(menu, principalWin);
            Result(Constants.SELECTED_INDEX_ORIGIN, 0);
            dialogOpen = false;
            Application.Run();

        }

        // Manage Tags
        internal void AddTag()
        {
            if (!dialogOpen)
            {
                dialogOpen = true;
                if (origins.Count() > 0)
                {
                    int pos = originView.GetSelectedIndex();
                    AddTag view = new(origins.ToList()[pos]);
                    view.AddSubscriber(this);
                    View dialog = view.render();
                    principalWin.Add(dialog);
                    dialog.SetFocus();
                }
                else
                {
                    Filler.NoOrigin();
                }
            }
        }

        internal void ModifyTag()
        {
            if (!dialogOpen)
            {
                dialogOpen = true;
                if (origins.Count() > 0)
                {
                    int pos = originView.GetSelectedIndex();
                    DataOrigin origin = origins.ToList()[pos];
                    List<DataTag> tagsLis = tags.Where(t => t.DataOrigin.Equals(origin)).ToList();
                    if (tagsLis.Count() > 0)
                    {
                        pos = tagView.GetSelectedIndex();
                        AddTag view = new(origin, tagsLis[pos]);
                        view.AddSubscriber(this);
                        View dialog = view.render();
                        principalWin.Add(dialog);
                        dialog.SetFocus();
                    }
                    else
                    {
                        Filler.NoTag();
                    }
                }
                else
                {
                    Filler.NoOrigin();
                }
                dialogOpen = false;
            }
        }

        internal void RemoveTag()
        {
            if (!dialogOpen)
            {
                dialogOpen = true;
                if (origins.Count() > 0)
                {
                    int pos = originView.GetSelectedIndex();
                    DataOrigin origin = origins.ToList()[pos];
                    List<DataTag> tagsList = tags.Where(t => t.DataOrigin.Equals(origin)).ToList();
                    int selected = tagView.GetSelectedIndex();

                    if (tagsList.Count() > 0)
                    {
                        pos = tagView.GetSelectedIndex();
                        int ris = MessageBox.Query(50, 7,
                           "Delete", "Are you sure you want to delete " + tagsList[selected].Name + "?", "Yes", "No");

                        if (ris == 0)
                        {
                            _ = tags.Remove(tagsList[selected]);
                            _ = dbOrigin.SaveChanges();
                            tagView.UpdateList(tags.Where(t => t.DataOrigin.Equals(origin)));
                            principalWin.Redraw(principalWin.Bounds);
                            _ = dbOrigin.SaveChanges();

                        }
                    }
                    else
                    {
                        Filler.NoTag();
                    }
                }
                else
                {
                    Filler.NoOrigin();
                }
                dialogOpen = false;
            }
        }


        // Manage Origins
        public void AddOrigin()
        {
            if (!dialogOpen)
            {
                dialogOpen = true;
                AddOrigin view = new();
                view.AddSubscriber(this);
                View dialog = view.render();
                principalWin.Add(dialog);
                dialog.SetFocus();
            }
        }

        public void RemoveOrigin()
        {
            List<DataOrigin> originList = origins.ToList();
            if (!dialogOpen)
            {
                dialogOpen = true;
                if (origins.Count() > 0)
                {
                    int pos = originView.GetSelectedIndex();

                    int ris = MessageBox.Query(50, 7,
                            "Delete", "Are you sure you want to delete " + originList[pos].Name + "?", "Yes", "No");

                    if (ris == 0)
                    {
                        _ = origins.Remove(originList[pos]);
                        _ = dbOrigin.SaveChanges();
                        originView.UpdateList(origins.ToList());
                        principalWin.Redraw(principalWin.Bounds);
                    }
                }
                else
                {
                    Filler.NoOrigin();
                }
                dialogOpen = false;
            }
        }

        // Results from other Views
        public void Result(int from, object result)
        {
            switch (from)
            {
                // Origin
                case Constants.ADD_DATA_ORIGIN:
                    DataOrigin tmpDO = (DataOrigin)result;
                    if (!origins.Contains(tmpDO))
                    {
                        _ = origins.Add(tmpDO);
                        _ = dbOrigin.SaveChanges();
                        originView.UpdateList(origins.ToList());
                        dialogOpen = true;

                    }
                    else
                    {
                        _ = MessageBox.Query(50, 7,
                        "ERROR", "Data origin with same IP, Path already exists!", "OK");
                        dialogOpen = false;
                    }
                    break;

                case Constants.ADD_DATA_TAG:
                    dialogOpen = true;
                    List<DataOrigin> originList = origins.ToList();

                    DataTag tmpDT = (DataTag)result;
                    DataOrigin selected = originList[originView.GetSelectedIndex()];
                    tmpDT.DataOrigin = selected;
                    _ = tags.Contains(tmpDT) ? tags.Update(tmpDT) : tags.Add(tmpDT);
                    _ = dbOrigin.SaveChanges();
                    tagView.UpdateList(tags.Where(t => t.DataOrigin.Equals(selected)));

                    break;


                case Constants.SELECTED_INDEX_ORIGIN:
                    originList = origins.ToList();

                    if (originList.Count > 0 && originList.Count > (int)result)
                    {
                        DataOrigin origin = originList[(int)result];
                        IQueryable<DataTag>? dataTags = tags.Where(t => t.DataOrigin.Equals(origin));
                        List<DataTag>? dataTagsList = dataTags.ToList();
                        tagView.UpdateList(dataTagsList);

                        if (dataTagsList.Count > 0)
                        {
                            liveView.UpdateLive(dataTagsList[tagView.GetSelectedIndex()]);
                        }
                        else
                        {
                            liveView.UpdateLive(null);
                        }
                    }



                    break;


                case Constants.SELECTED_INDEX_TAG:
                    int pos = originView.GetSelectedIndex();
                    originList = origins.ToList();
                    List<DataTag> tagsList = tags.Where(t => t.DataOrigin.Equals(originList[pos])).ToList();
                    int pos2 = tagView.GetSelectedIndex();

                    if (origins.Count() > 0 && tagsList.Count > 0 && tagsList.Count > pos2)
                    {
                        liveView.UpdateLive(tagsList[pos2]);
                    }
                    break;


                case Constants.CLOSE:
                    if (dialogOpen)
                    {
                        principalWin.Remove((View)result);
                        menu.SetFocus();
                        dialogOpen = false;
                    }
                    break;


                case Constants.EXIT:
                    principalWin.Remove((View)result);
                    menu.SetFocus();
                    dialogOpen = false;
                    break;
            }
        }
    }

}
