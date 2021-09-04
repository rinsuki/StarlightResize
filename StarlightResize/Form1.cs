using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace StarlightResize
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            ReloadDisplayList();
        }

        private void ReloadDisplayList()
        {
            comboBoxDisplay.Items.Clear();
            foreach (var screen in Screen.AllScreens)
            {
                comboBoxDisplay.Items.Add(screen);
            }
        }

        private void buttonResize_Click(object sender, EventArgs e)
        {
            var screen = comboBoxDisplay.SelectedItem as Screen;
            if (screen == null)
            {
                MessageBox.Show("ディスプレイが指定されていません。\n先にデレステを表示するディスプレイを選択してください。", "StarlightResize", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var process = Process.GetProcessesByName("imascgstage").FirstOrDefault();
            if (process == null)
            {
                MessageBox.Show("デレステのウィンドウが見つかりませんでした。\nデレステが起動していることを確認してください。", "StarlightResize", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var width = (int)numericUpDownWidth.Value;
            var height = (int)numericUpDownHeight.Value;

            var hWnd = (HWND)process.MainWindowHandle;
            PInvoke.GetWindowRect(hWnd, out var windowRect);
            PInvoke.GetClientRect(hWnd, out var clientRect);
            var screenPoint = new POINT();
            PInvoke.ClientToScreen(hWnd, ref screenPoint);
            var windowWidth = windowRect.right - windowRect.left;
            var clientWidth = clientRect.right - clientRect.left;
            var frameWidth = windowWidth - clientWidth;
            var windowHeight = windowRect.bottom - windowRect.top;
            var clientHeight = clientRect.bottom - clientRect.top;
            var frameHeight = windowHeight - clientHeight;

            var newPoint = new POINT();
            newPoint.x = screen.Bounds.X;
            newPoint.y = screen.Bounds.Y;
            if (radioButtonPosCenter.Checked)
            {
                // 中央寄せ
                newPoint.x += (screen.Bounds.Width - width) / 2;
                newPoint.y += (screen.Bounds.Height - height) / 2;
            } else
            {
                if (radioButtonPosRightBottom.Checked || radioButtonPosLeftBottom.Checked)
                {
                    // 下寄せ
                    newPoint.y += screen.Bounds.Height - height;
                }
                if (radioButtonPosRightTop.Checked || radioButtonPosRightBottom.Checked)
                {
                    // 右寄せ
                    newPoint.x += screen.Bounds.Width - width;
                }
            }
            newPoint.x += windowRect.left - screenPoint.x;
            newPoint.y += windowRect.top - screenPoint.y;

            // 違うDPIのモニタからウィンドウを移動するとデレステ側？でDPIの差分からのウィンドウサイズ補正がかかる
            // ので2回リサイズ処理を行う
            PInvoke.MoveWindow(hWnd, newPoint.x, newPoint.y, width + frameWidth, height + frameHeight, true);
            PInvoke.MoveWindow(hWnd, newPoint.x, newPoint.y, width + frameWidth, height + frameHeight, true);
        }

        private void SetResolution(int width, int height)
        {
            numericUpDownWidth.Value = width;
            numericUpDownHeight.Value = height;
        }

        private void buttonSetResTo1280_Click(object sender, EventArgs e)
        {
            SetResolution(1280, 720);
        }

        private void buttonSetResTo1920_Click(object sender, EventArgs e)
        {
            SetResolution(1920, 1080);
        }

        private void buttonSetResTo2560_Click(object sender, EventArgs e)
        {
            SetResolution(2560, 1440);
        }

        private void buttonSetResTo3840_Click(object sender, EventArgs e)
        {
            SetResolution(3840, 2160);
        }

        private void buttonSetResToDisplay_Click(object sender, EventArgs e)
        {
            var screen = comboBoxDisplay.SelectedItem as Screen;
            if (screen == null)
            {
                MessageBox.Show("ディスプレイが指定されていません。\n先にデレステを表示するディスプレイを選択してください", "StarlightResize", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            SetResolution(screen.Bounds.Width, screen.Bounds.Height);
        }
    }
}
