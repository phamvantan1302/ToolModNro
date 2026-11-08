using System.Collections.Generic;
using UnityEngine;
using Mod;

public class BossNotificationPanel
{
    private bool isVisible = false;
    private int scrollIndex = 0;
    private int selectedBossIndex = -1;
    private const int PanelWidth = 280;
    private const int PanelHeight = 400;
    private const int LineHeight = 18;
    private int panelX = 20;
    private int panelY = 40;

    // Lấy danh sách boss hiển thị (kết hợp bossNotif và bossList, không trùng)
    private MyVector GetDisplayList()
    {
        MyVector displayList = new MyVector();

        // Thêm từ bossNotif (thông báo gần đây)
        for (int i = 0; i < ModFunc.bossNotif.size(); i++)
        {
            ShowBoss boss = (ShowBoss)ModFunc.bossNotif.elementAt(i);
            if (boss != null)
            {
                displayList.addElement(boss);
            }
        }

        // Thêm từ bossList (boss đã lưu), bỏ trùng
        for (int i = 0; i < ModFunc.bossList.size(); i++)
        {
            ShowBoss boss = (ShowBoss)ModFunc.bossList.elementAt(i);
            if (boss != null)
            {
                bool exists = false;
                for (int j = 0; j < displayList.size(); j++)
                {
                    ShowBoss existing = (ShowBoss)displayList.elementAt(j);
                    if (existing != null && existing.nameBoss == boss.nameBoss && existing.mapID == boss.mapID)
                    {
                        exists = true;
                        break;
                    }
                }
                if (!exists)
                {
                    displayList.addElement(boss);
                }
            }
        }

        return displayList;
    }

    public void Show()
    {
        isVisible = true;
        scrollIndex = 0;
        selectedBossIndex = -1;
    }

    public void Hide()
    {
        isVisible = false;
        selectedBossIndex = -1;
    }

    public bool IsVisible()
    {
        return isVisible;
    }

    public void Paint(mGraphics g)
    {
        if (!isVisible)
            return;

        MyVector displayList = GetDisplayList();

        // Vẽ nền panel
        g.setColor(0);
        g.fillRect(panelX, panelY, PanelWidth, PanelHeight);

        // Vẽ viền
        g.setColor(16711680);
        g.drawRect(panelX, panelY, PanelWidth, PanelHeight);

        // Vẽ tiêu đề
        mFont.tahoma_7b_white.drawString(g, "THÔNG BÁO BOSS", panelX + PanelWidth / 2, panelY + 8, mFont.CENTER);

        // Vẽ nút X đóng
        mFont.tahoma_7b_red.drawString(g, "X", panelX + PanelWidth - 12, panelY + 8, mFont.CENTER);

        if (selectedBossIndex >= 0 && selectedBossIndex < displayList.size())
        {
            PaintBossDetail(g, displayList);
        }
        else
        {
            PaintBossList(g, displayList);
        }
    }

    private void PaintBossList(mGraphics g, MyVector displayList)
    {
        int maxLines = (PanelHeight - 60) / LineHeight;
        int startY = panelY + 30;
        int lineCount = 0;

        if (displayList.size() == 0)
        {
            mFont.tahoma_7_white.drawString(g, "Chưa có thông báo boss nào.", panelX + 10, startY, mFont.LEFT);
        }

        for (int i = scrollIndex; i < displayList.size() && lineCount < maxLines; i++)
        {
            ShowBoss boss = (ShowBoss)displayList.elementAt(i);
            if (boss != null)
            {
                int y = startY + lineCount * LineHeight;

                // Vẽ nền item
                g.setColor(2721889);
                g.fillRect(panelX + 5, y - 2, PanelWidth - 10, LineHeight - 2);

                // Vẽ text với thời gian
                string timeAgo = NinjaUtil.getTimeAgo(boss.time / 1000);
                string text = boss.nameBoss + " - " + boss.mapName;
                if (text.Length > 28)
                    text = text.Substring(0, 25) + "...";
                text += " " + timeAgo;

                // Boss cùng map hiện tại -> hiển thị màu đỏ
                if (TileMap.mapName != null && TileMap.mapName.Trim().ToLower() == boss.mapName.Trim().ToLower())
                {
                    mFont.tahoma_7_red.drawString(g, text, panelX + 10, y, mFont.LEFT);
                }
                else
                {
                    mFont.tahoma_7_white.drawString(g, text, panelX + 10, y, mFont.LEFT);
                }
                lineCount++;
            }
        }

        // Vẽ tổng số boss
        mFont.tahoma_7_yellow.drawString(g, "Tổng: " + displayList.size() + " boss", panelX + 10, panelY + PanelHeight - 15, mFont.LEFT);

        // Vẽ mũi tên scroll nếu cần
        if (scrollIndex > 0)
        {
            mFont.tahoma_7_white.drawString(g, "▲", panelX + PanelWidth - 15, startY - 5, mFont.CENTER);
        }
        if (scrollIndex + maxLines < displayList.size())
        {
            mFont.tahoma_7_white.drawString(g, "▼", panelX + PanelWidth - 15, panelY + PanelHeight - 25, mFont.CENTER);
        }
    }

    private void PaintBossDetail(mGraphics g, MyVector displayList)
    {
        ShowBoss boss = (ShowBoss)displayList.elementAt(selectedBossIndex);
        if (boss == null)
            return;

        int startY = panelY + 30;
        int lineHeight = 18;

        // Vẽ nút quay lại
        g.setColor(2721889);
        g.fillRect(panelX + 5, startY - 2, 80, 16);
        mFont.tahoma_7b_white.drawString(g, "< Quay lại", panelX + 10, startY, mFont.LEFT);

        // Vẽ chi tiết boss
        startY += 25;
        mFont.tahoma_7b_yellow.drawString(g, "Tên Boss:", panelX + 10, startY, mFont.LEFT);
        startY += lineHeight;
        mFont.tahoma_7_white.drawString(g, boss.nameBoss, panelX + 15, startY, mFont.LEFT);

        startY += lineHeight + 5;
        mFont.tahoma_7b_yellow.drawString(g, "Map:", panelX + 10, startY, mFont.LEFT);
        startY += lineHeight;
        mFont.tahoma_7_white.drawString(g, "ID: " + boss.mapID + " - " + boss.mapName, panelX + 15, startY, mFont.LEFT);

        startY += lineHeight + 5;
        mFont.tahoma_7b_yellow.drawString(g, "Khu:", panelX + 10, startY, mFont.LEFT);
        startY += lineHeight;
        string zoneText = boss.zoneID >= 0 ? "Khu " + boss.zoneID : "Chưa biết";
        mFont.tahoma_7_white.drawString(g, zoneText, panelX + 15, startY, mFont.LEFT);

        startY += lineHeight + 5;
        string timeAgo = NinjaUtil.getTimeAgo(boss.time / 1000);
        mFont.tahoma_7b_yellow.drawString(g, "Thời gian:", panelX + 10, startY, mFont.LEFT);
        startY += lineHeight;
        mFont.tahoma_7_white.drawString(g, timeAgo, panelX + 15, startY, mFont.LEFT);

        // Nút dò boss
        startY += lineHeight + 15;
        g.setColor(2721889);
        g.fillRect(panelX + 10, startY - 2, 120, 18);
        g.setColor(16711680);
        g.drawRect(panelX + 10, startY - 2, 120, 18);
        mFont.tahoma_7b_red.drawString(g, "Bấm để dò boss", panelX + 15, startY + 2, mFont.LEFT);
    }

    public void Update()
    {
        if (!isVisible)
            return;

        MyVector displayList = GetDisplayList();
        int maxLines = (PanelHeight - 60) / LineHeight;

        // Scroll lên
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            scrollIndex--;
            if (scrollIndex < 0)
                scrollIndex = 0;
        }

        // Scroll xuống
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            scrollIndex++;
            if (scrollIndex > Mathf.Max(0, displayList.size() - maxLines))
                scrollIndex = Mathf.Max(0, displayList.size() - maxLines);
        }

        // Quay lại từ chi tiết
        if (selectedBossIndex >= 0 && Input.GetKeyDown(KeyCode.LeftArrow))
        {
            selectedBossIndex = -1;
        }

        // Ẩn panel
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Hide();
        }
    }

    public void HandleClick(int x, int y)
    {
        if (!isVisible)
            return;

        // Click ngoài panel -> đóng panel
        if (x < panelX || x > panelX + PanelWidth || y < panelY || y > panelY + PanelHeight)
        {
            Hide();
            return;
        }

        // Click nút X đóng
        if (x >= panelX + PanelWidth - 20 && x <= panelX + PanelWidth && y >= panelY && y <= panelY + 20)
        {
            Hide();
            return;
        }

        MyVector displayList = GetDisplayList();

        if (selectedBossIndex >= 0)
        {
            // Đang xem chi tiết
            int startY = panelY + 30;

            // Kiểm tra click vào nút quay lại
            if (x >= panelX + 5 && x <= panelX + 85 && y >= startY - 2 && y <= startY + 14)
            {
                selectedBossIndex = -1;
                return;
            }

            // Bấm để dò boss
            if (selectedBossIndex < displayList.size())
            {
                ShowBoss boss = (ShowBoss)displayList.elementAt(selectedBossIndex);
                // Tính vị trí nút dò boss (tương ứng với PaintBossDetail)
                int detailY = panelY + 30 + 25 + 18 + 18 + 5 + 18 + 18 + 5 + 18 + 18 + 5 + 18 + 18 + 15;
                if (boss != null && x >= panelX + 10 && x <= panelX + 130 && y >= detailY - 2 && y <= detailY + 16)
                {
                    ModFunc.GI().StartBossSearch(boss);
                    Hide();
                }
            }
        }
        else
        {
            // Đang xem danh sách, kiểm tra click vào boss
            int maxLines = (PanelHeight - 60) / LineHeight;
            int startY = panelY + 30;
            int lineCount = 0;

            // Click mũi tên scroll lên
            if (scrollIndex > 0 && x >= panelX + PanelWidth - 25 && y >= startY - 10 && y <= startY)
            {
                scrollIndex--;
                return;
            }

            // Click mũi tên scroll xuống
            if (scrollIndex + maxLines < displayList.size() && x >= panelX + PanelWidth - 25 && y >= panelY + PanelHeight - 30 && y <= panelY + PanelHeight - 15)
            {
                scrollIndex++;
                return;
            }

            for (int i = scrollIndex; i < displayList.size() && lineCount < maxLines; i++)
            {
                ShowBoss boss = (ShowBoss)displayList.elementAt(i);
                if (boss != null)
                {
                    int itemY = startY + lineCount * LineHeight;
                    if (x >= panelX + 5 && x <= panelX + PanelWidth - 5 && y >= itemY - 2 && y <= itemY + LineHeight - 2)
                    {
                        selectedBossIndex = i;
                        // Cũng thêm vào bossList khi click
                        ModFunc.GI().AddBossToList(boss);
                        return;
                    }
                    lineCount++;
                }
            }
        }
    }
}
