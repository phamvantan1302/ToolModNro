using System.Collections.Generic;
using UnityEngine;
using Mod;

public class BossListPanel
{
    private bool isVisible = false;
    private int scrollIndex = 0;
    private const int PanelWidth = 220;
    private const int PanelHeight = 320;
    private const int LineHeight = 15;
    private int panelX = 10;
    private int panelY = 50;

    public void Show()
    {
        isVisible = true;
        scrollIndex = 0;
    }

    public void Hide()
    {
        isVisible = false;
    }

    public bool IsVisible()
    {
        return isVisible;
    }

    public void Paint(mGraphics g)
    {
        if (!isVisible)
            return;

        // Vẽ nền panel
        g.setColor(0);
        g.fillRect(panelX, panelY, PanelWidth, PanelHeight);

        // Vẽ viền
        g.setColor(16711680);
        g.drawRect(panelX, panelY, PanelWidth, PanelHeight);

        // Vẽ tiêu đề
        mFont.tahoma_7b_white.drawString(g, "DANH SÁCH BOSS", panelX + PanelWidth / 2, panelY + 5, mFont.CENTER);

        // Vẽ danh sách boss
        int maxLines = (PanelHeight - 40) / LineHeight;
        int startY = panelY + 20;
        int lineCount = 0;

        for (int i = scrollIndex; i < ModFunc.bossList.size() && lineCount < maxLines; i++)
        {
            ShowBoss boss = (ShowBoss)ModFunc.bossList.elementAt(i);
            if (boss != null)
            {
                string text = boss.nameBoss + " (Map " + boss.mapID + ")";
                if (text.Length > 28)
                    text = text.Substring(0, 25) + "...";

                mFont.tahoma_7_white.drawString(g, text, panelX + 5, startY + lineCount * LineHeight, mFont.LEFT);
                lineCount++;
            }
        }

        // Vẽ số boss
        mFont.tahoma_7_yellow.drawString(g, "Tổng: " + ModFunc.bossList.size(), panelX + 5, panelY + PanelHeight - 10, mFont.LEFT);
    }

    public void Update()
    {
        if (!isVisible)
            return;

        int maxLines = (PanelHeight - 40) / LineHeight;

        // Scroll lên (phím mũi tên trên)
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            scrollIndex--;
            if (scrollIndex < 0)
                scrollIndex = 0;
        }

        // Scroll xuống (phím mũi tên dưới)
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            scrollIndex++;
            if (scrollIndex > Mathf.Max(0, ModFunc.bossList.size() - maxLines))
                scrollIndex = Mathf.Max(0, ModFunc.bossList.size() - maxLines);
        }

        // Ẩn panel khi ấn ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Hide();
        }
    }
}
