// ============================================================
//  ONAIR.exe — телефон экранын Windows-та көрсету (iPhone + Android)
//  ONAIR бренд дизайны: dark-first, көк→күлгін gradient акцент, pill, glass, glow.
//  iPhone -> UxPlay (AirPlay) · Android -> scrcpy (Wi-Fi/USB, adb). Екеуі қатар.
//  WinExe => қара консоль жоқ. UTF-8 (/codepage:65001).
// ============================================================
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace OnAirApp {
  static class Program {
    [STAThread]
    static void Main() {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run(new MainForm());
    }
  }

  // ── Бренд палитрасы ───────────────────────────────────────
  static class Brand {
    public static Color Bg     = Color.FromArgb(7, 7, 10);
    public static Color Bg1    = Color.FromArgb(12, 12, 17);
    public static Color Bar    = Color.FromArgb(11, 11, 15);
    public static Color Surf   = Color.FromArgb(24, 24, 30);
    public static Color Surf2  = Color.FromArgb(31, 31, 39);
    public static Color Hair   = Color.FromArgb(38, 38, 48);
    public static Color Hair2  = Color.FromArgb(54, 54, 66);
    public static Color Fg     = Color.FromArgb(244, 244, 247);
    public static Color FgMute = Color.FromArgb(167, 167, 180);
    public static Color FgFaint= Color.FromArgb(106, 106, 120);
    public static Color Blue   = Color.FromArgb(63, 182, 255);
    public static Color Indigo = Color.FromArgb(97, 145, 244);
    public static Color Viobl  = Color.FromArgb(159, 110, 248);
    public static Color Violet = Color.FromArgb(181, 95, 250);
    public static Color Live   = Color.FromArgb(255, 77, 94);
    public static Color Mint   = Color.FromArgb(70, 214, 160);

    public static GraphicsPath Round(Rectangle r, int rad) {
      int d = rad * 2; if (d > r.Height) d = r.Height; if (d > r.Width) d = r.Width;
      GraphicsPath p = new GraphicsPath();
      p.AddArc(r.X, r.Y, d, d, 180, 90);
      p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
      p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
      p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
      p.CloseFigure(); return p;
    }
    public static LinearGradientBrush Grad(Rectangle r) {
      if (r.Width < 1) r.Width = 1; if (r.Height < 1) r.Height = 1;
      LinearGradientBrush b = new LinearGradientBrush(r, Blue, Violet, 18f);
      ColorBlend cb = new ColorBlend(4);
      cb.Colors = new Color[] { Blue, Indigo, Viobl, Violet };
      cb.Positions = new float[] { 0f, 0.38f, 0.72f, 1f };
      b.InterpolationColors = cb;
      return b;
    }
    public static Color Lighten(Color c, int a) {
      return Color.FromArgb(Math.Min(255, c.R + a), Math.Min(255, c.G + a), Math.Min(255, c.B + a));
    }
  }

  // ── Дөңгелек/pill батырма (Solid / Gradient / Ghost) ──────
  class RoundButton : Button {
    public enum Mode { Solid, Gradient, Ghost }
    public Mode Style = Mode.Solid;
    public int Radius = 12;
    public Color Border = Brand.Hair2;
    public enum Ico { None, Plus, Dot, Square, TriLeft, Circle, SquareO, Power, Minus, Info }
    public Ico Shape = Ico.None;
    bool hot = false;
    public RoundButton() {
      SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
             | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
      FlatStyle = FlatStyle.Flat; FlatAppearance.BorderSize = 0;
      MouseEnter += delegate { hot = true; Invalidate(); };
      MouseLeave += delegate { hot = false; Invalidate(); };
    }
    protected override void OnPaint(PaintEventArgs e) {
      Graphics g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
      g.Clear((Parent != null) ? Parent.BackColor : BackColor);
      Rectangle r = new Rectangle(0, 0, Width - 1, Height - 1);
      using (GraphicsPath p = Brand.Round(r, Radius)) {
        if (Style == Mode.Gradient) {
          using (Brush b = Brand.Grad(r)) g.FillPath(b, p);
          if (hot) using (Brush b = new SolidBrush(Color.FromArgb(28, 255, 255, 255))) g.FillPath(b, p);
        } else if (Style == Mode.Ghost) {
          using (Brush b = new SolidBrush(hot ? Brand.Lighten(BackColor, 12) : BackColor)) g.FillPath(b, p);
          using (Pen pen = new Pen(hot ? Brand.Hair2 : Border, 1.3f)) g.DrawPath(pen, p);
        } else {
          using (Brush b = new SolidBrush(hot ? Brand.Lighten(BackColor, 18) : BackColor)) g.FillPath(b, p);
        }
      }
      if (Shape == Ico.None) {
        // Мәтінді дәл өлшеп, тура ортаға қою
        Size ts = TextRenderer.MeasureText(g, Text, Font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding);
        Point tp = new Point((Width - ts.Width) / 2, (Height - ts.Height) / 2);
        TextRenderer.DrawText(g, Text, Font, tp, ForeColor, TextFormatFlags.NoPadding);
      } else {
        // Векторлық иконка — пиксельге дәл орта
        float cx = Width / 2f, cy = Height / 2f;
        if (Shape == Ico.Plus) {
          using (Pen pen = new Pen(ForeColor, 2.6f)) {
            pen.StartCap = LineCap.Round; pen.EndCap = LineCap.Round;
            g.DrawLine(pen, cx - 8, cy, cx + 8, cy);
            g.DrawLine(pen, cx, cy - 8, cx, cy + 8);
          }
        } else if (Shape == Ico.Dot) {
          using (Brush b = new SolidBrush(ForeColor)) g.FillEllipse(b, cx - 6f, cy - 6f, 12f, 12f);
        } else if (Shape == Ico.Square) {
          using (Brush b = new SolidBrush(ForeColor))
          using (GraphicsPath sp = Brand.Round(new Rectangle((int)(cx - 6), (int)(cy - 6), 12, 12), 2))
            g.FillPath(b, sp);
        } else if (Shape == Ico.TriLeft) {            // Артқа
          using (Brush b = new SolidBrush(ForeColor))
            g.FillPolygon(b, new PointF[] { new PointF(cx - 5, cy), new PointF(cx + 5, cy - 7), new PointF(cx + 5, cy + 7) });
        } else if (Shape == Ico.Circle) {             // Үй
          using (Pen pen = new Pen(ForeColor, 2f)) g.DrawEllipse(pen, cx - 7, cy - 7, 14, 14);
        } else if (Shape == Ico.SquareO) {            // Терезелер
          using (Pen pen = new Pen(ForeColor, 2f)) g.DrawRectangle(pen, (int)(cx - 6), (int)(cy - 6), 12, 12);
        } else if (Shape == Ico.Power) {              // Қуат
          using (Pen pen = new Pen(ForeColor, 2f)) {
            pen.StartCap = LineCap.Round; pen.EndCap = LineCap.Round;
            g.DrawArc(pen, cx - 7, cy - 6, 14, 14, -65, 310);
            g.DrawLine(pen, cx, cy - 9, cx, cy - 1);
          }
        } else if (Shape == Ico.Minus) {              // Дыбыс −
          using (Pen pen = new Pen(ForeColor, 2.6f)) { pen.StartCap = LineCap.Round; pen.EndCap = LineCap.Round; g.DrawLine(pen, cx - 8, cy, cx + 8, cy); }
        } else if (Shape == Ico.Info) {                // Ақпарат
          using (Pen pen = new Pen(ForeColor, 1.8f)) {
            pen.StartCap = LineCap.Round; pen.EndCap = LineCap.Round;
            g.DrawEllipse(pen, cx - 8, cy - 8, 16, 16);
            g.DrawLine(pen, cx, cy - 1, cx, cy + 5);
          }
          using (Brush b = new SolidBrush(ForeColor)) g.FillEllipse(b, cx - 1.4f, cy - 6f, 2.8f, 2.8f);
        }
      }
    }
  }

  // ── Дөңгелек өріс контейнері ──────────────────────────────
  class RoundPanel : Panel {
    public int Radius = 12;
    public Color Fill = Brand.Bg1;
    public Color Border = Brand.Hair2;
    public RoundPanel() {
      SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
             | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
    }
    protected override void OnPaint(PaintEventArgs e) {
      Graphics g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
      g.Clear((Parent != null) ? Parent.BackColor : BackColor);
      Rectangle r = new Rectangle(0, 0, Width - 1, Height - 1);
      using (GraphicsPath p = Brand.Round(r, Radius)) {
        using (Brush b = new SolidBrush(Fill)) g.FillPath(b, p);
        using (Pen pen = new Pen(Border, 1.4f)) g.DrawPath(pen, p);
      }
    }
  }

  // ── Double-buffered панель (видео аймағы) ─────────────────
  class DBPanel : Panel {
    public DBPanel() { DoubleBuffered = true; ResizeRedraw = true; }
  }

  // Кадрды түсіріп, үстіне сурет салатын редактор (қарапайым терезе — сенімді)
  class AnnotEditor : Form {
    Bitmap img;
    System.Collections.Generic.List<Point[]> strokes = new System.Collections.Generic.List<Point[]>();
    System.Collections.Generic.List<Color> cols = new System.Collections.Generic.List<Color>();
    System.Collections.Generic.List<Point> cur = null;
    Color pen; float pw = 6f;
    DBPanel canvas; Panel bottom; string saveDir;
    RoundButton[] swatches;

    public AnnotEditor(Bitmap image, Color[] palette, string dir, bool light, string clearTxt, string saveTxt, string closeTxt) {
      img = image; pen = palette[0]; saveDir = dir;
      Text = "ONAIR";
      Rectangle wa = Screen.PrimaryScreen.WorkingArea;
      double ar = (double)img.Width / img.Height;
      int h = Math.Min(820, (int)(wa.Height * 0.82)); int w = (int)(h * ar) ;
      if (w > wa.Width * 0.9) { w = (int)(wa.Width * 0.9); h = (int)(w / ar); }
      ClientSize = new Size(Math.Max(360, w), h + 56);
      StartPosition = FormStartPosition.CenterScreen; FormBorderStyle = FormBorderStyle.Sizable;
      BackColor = Brand.Bg; Font = new Font("Segoe UI", 9);

      canvas = new DBPanel(); canvas.Dock = DockStyle.Fill; canvas.BackColor = Color.Black;
      canvas.Paint += delegate (object s, PaintEventArgs e) { Render(e.Graphics, canvas.Size); };
      canvas.MouseDown += delegate (object s, MouseEventArgs e) { if (e.Button == MouseButtons.Left) { cur = new System.Collections.Generic.List<Point>(); cur.Add(e.Location); canvas.Invalidate(); } };
      canvas.MouseMove += delegate (object s, MouseEventArgs e) { if (cur != null) { cur.Add(e.Location); canvas.Invalidate(); } };
      canvas.MouseUp += delegate (object s, MouseEventArgs e) { if (cur != null) { if (cur.Count > 0) { strokes.Add(cur.ToArray()); cols.Add(pen); } cur = null; canvas.Invalidate(); } };
      Controls.Add(canvas);

      bottom = new Panel(); bottom.Dock = DockStyle.Bottom; bottom.Height = 56; bottom.BackColor = Brand.Bar;
      bottom.Paint += delegate (object s, PaintEventArgs e) { using (Pen pn = new Pen(Brand.Hair, 1)) e.Graphics.DrawLine(pn, 0, 0, bottom.Width, 0); };
      Controls.Add(bottom); canvas.BringToFront();

      swatches = new RoundButton[palette.Length];
      int x = 14;
      for (int i = 0; i < palette.Length; i++) {
        RoundButton cb = new RoundButton(); cb.Style = RoundButton.Mode.Solid; cb.Radius = 17; cb.Size = new Size(34, 34);
        cb.BackColor = palette[i]; cb.Cursor = Cursors.Hand; cb.Location = new Point(x, 11);
        int ci = i; cb.Click += delegate { pen = palette[ci]; };
        bottom.Controls.Add(cb); swatches[i] = cb; x += 40;
      }
      RoundButton bSave = MkBtn(saveTxt, Brand.Mint, true);
      RoundButton bClear = MkBtn(clearTxt, Brand.Surf, false);
      RoundButton bClose = MkBtn(closeTxt, Brand.Surf, false);
      bClear.Click += delegate { strokes.Clear(); cols.Clear(); cur = null; canvas.Invalidate(); };
      bSave.Click += delegate { Save(); };
      bClose.Click += delegate { Close(); };
      bottom.Controls.Add(bClear); bottom.Controls.Add(bSave); bottom.Controls.Add(bClose);
      bottom.Resize += delegate { int rx = bottom.Width - 12; bClose.Location = new Point(rx - bClose.Width, 11); rx -= bClose.Width + 8; bSave.Location = new Point(rx - bSave.Width, 11); rx -= bSave.Width + 8; bClear.Location = new Point(rx - bClear.Width, 11); };
      this.Shown += delegate { try { int v = light ? 0 : 1; DwmSetWindowAttribute2(this.Handle, 20, ref v, 4); } catch {} bottom.PerformLayout(); int rx = bottom.Width - 12; bClose.Location = new Point(rx - bClose.Width, 11); rx -= bClose.Width + 8; bSave.Location = new Point(rx - bSave.Width, 11); rx -= bSave.Width + 8; bClear.Location = new Point(rx - bClear.Width, 11); };
    }
    [DllImport("dwmapi.dll")] static extern int DwmSetWindowAttribute2(IntPtr h, int a, ref int v, int s);
    RoundButton MkBtn(string t, Color bg, bool grad) {
      RoundButton b = new RoundButton(); b.Text = t; b.Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold);
      b.Style = grad ? RoundButton.Mode.Gradient : RoundButton.Mode.Ghost; b.Radius = 17; b.Border = Brand.Hair;
      Size m = TextRenderer.MeasureText(t, b.Font); b.Size = new Size(Math.Max(80, m.Width + 28), 34); b.Location = new Point(0, 11);
      b.BackColor = bg; b.ForeColor = grad ? Color.White : Brand.Fg; b.Cursor = Cursors.Hand; return b;
    }
    Rectangle FitRect(Size area) {
      double ar = (double)img.Width / img.Height;
      int w = area.Width, h = (int)(w / ar);
      if (h > area.Height) { h = area.Height; w = (int)(h * ar); }
      return new Rectangle((area.Width - w) / 2, (area.Height - h) / 2, w, h);
    }
    void Render(Graphics g, Size area) {
      g.SmoothingMode = SmoothingMode.AntiAlias; g.InterpolationMode = InterpolationMode.HighQualityBicubic;
      g.Clear(Color.Black);
      g.DrawImage(img, FitRect(area));
      for (int i = 0; i < strokes.Count; i++) Stroke(g, strokes[i], cols[i]);
      if (cur != null) Stroke(g, cur.ToArray(), pen);
    }
    void Stroke(Graphics g, Point[] pts, Color c) {
      if (pts == null || pts.Length < 1) return;
      if (pts.Length == 1) { using (Brush b = new SolidBrush(c)) g.FillEllipse(b, pts[0].X - pw / 2, pts[0].Y - pw / 2, pw, pw); return; }
      using (Pen p = new Pen(c, pw)) { p.StartCap = LineCap.Round; p.EndCap = LineCap.Round; p.LineJoin = LineJoin.Round; g.DrawLines(p, pts); }
    }
    void Save() {
      try {
        System.IO.Directory.CreateDirectory(saveDir);
        string path = System.IO.Path.Combine(saveDir, "annot_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png");
        using (Bitmap b = new Bitmap(canvas.Width, canvas.Height))
        using (Graphics g = Graphics.FromImage(b)) { Render(g, canvas.Size); b.Save(path, System.Drawing.Imaging.ImageFormat.Png); }
        try { Process.Start("explorer.exe", "/select,\"" + path + "\""); } catch {}
        Close();
      } catch {}
    }
  }

  class MainForm : Form {
    [DllImport("dwmapi.dll")] static extern int DwmSetWindowAttribute(IntPtr h, int a, ref int v, int s);
    [DllImport("user32.dll")] static extern bool EnumWindows(EnumWindowsProc cb, IntPtr l);
    delegate bool EnumWindowsProc(IntPtr h, IntPtr l);
    [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr h, out uint pid);
    [DllImport("user32.dll")] static extern bool IsWindowVisible(IntPtr h);
    [DllImport("user32.dll")] static extern int GetWindowTextLength(IntPtr h);
    [DllImport("user32.dll")] static extern IntPtr SetParent(IntPtr c, IntPtr p);
    [DllImport("user32.dll")] static extern int GetWindowLong(IntPtr h, int i);
    [DllImport("user32.dll")] static extern int SetWindowLong(IntPtr h, int i, int v);
    [DllImport("user32.dll")] static extern bool MoveWindow(IntPtr h, int x, int y, int w, int ht, bool r);
    [DllImport("user32.dll")] static extern bool GetWindowRect(IntPtr h, out RECT r);
    [DllImport("user32.dll")] static extern bool PostMessage(IntPtr h, uint m, IntPtr w, IntPtr l);
    [DllImport("user32.dll")] static extern IntPtr MonitorFromWindow(IntPtr h, uint f);
    [DllImport("shcore.dll")] static extern int GetDpiForMonitor(IntPtr m, int t, out uint x, out uint y);
    const uint WM_CLOSE = 0x0010;
    struct RECT { public int Left, Top, Right, Bottom; }
    const int GWL_STYLE = -16;
    const uint WS_CHILD=0x40000000, WS_VISIBLE=0x10000000, WS_POPUP=0x80000000,
               WS_CAPTION=0x00C00000, WS_THICKFRAME=0x00040000,
               WS_MINIMIZEBOX=0x20000, WS_MAXIMIZEBOX=0x10000, WS_SYSMENU=0x80000;

    string baseDir, uxplay, scrcpyDir, scrcpy, adb, deviceFile, dataDir;
    string[] uxArgs = { "-n","ONAIR","-s","1920x1080","-vs","d3d11videosink","-as","wasapisink" };

    Process procIos, procAndroid, recProc;
    IntPtr  childIos = IntPtr.Zero, childAndroid = IntPtr.Zero;
    Size    natIos = Size.Empty, natAndroid = Size.Empty;
    bool    embIos = false, embAndroid = false;

    const string APP_VERSION = "1.0";
    Bitmap logoBmp;
    Font fDisplay, fWord, fBtn, fHint, fEyebrow, fMono;
    Panel bar; DBPanel video;
    RoundButton btnIos, btnAndroid, btnAdd, btnRec;
    Timer timer;

    bool recording = false;
    string recIos = null, recAndroid = null, recDir, recPath = null;

    Panel tools;
    RoundButton btnShot, btnRotate, btnFull;
    Timer shotFlash;
    // Аннотация (экранға сурет салу)
    bool annotating = false;
    RoundButton btnAnnot, btnClear, btnDone; RoundButton[] colorBtns;
    Color[] palette = { Color.FromArgb(232, 40, 52), Color.FromArgb(245, 200, 40), Color.FromArgb(46, 200, 120), Color.FromArgb(60, 150, 255), Color.White };
    int curColor = 0;
    bool fullscreen = false, androidLandscape = false;
    Rectangle savedBounds; FormBorderStyle savedBorder = FormBorderStyle.Sizable;

    // Баптаулар + тіл
    string lang = "kz";
    int aMaxSize = 0, aMaxFps = 60, aBitRate = 8, iosFps = 60;
    bool topMost = false, screenOff = false, light = false, recMic = false;
    Rectangle gearRect = Rectangle.Empty; bool gearHot = false;

    // Жазу таймері + авто-қайта қосылу
    DateTime recStart;
    bool androidWantOn = false, reconnecting = false;
    DateTime lastReconnectAt = DateTime.MinValue; int reconnTries = 0;

    // Android навигация + бірнеше телефон
    RoundButton btnBack, btnHome, btnRecents, btnPower, btnVolDn, btnVolUp, btnInfo;
    List<string> phones = new List<string>();

    public MainForm() {
      baseDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
      uxplay  = Path.Combine(baseDir, "bin", "uxplay.exe");
      scrcpyDir = Path.Combine(baseDir, "scrcpy");
      scrcpy = Path.Combine(scrcpyDir, "scrcpy.exe");
      adb    = Path.Combine(scrcpyDir, "adb.exe");
      // Пайдаланушы деректері — жазуға болатын қалтада (Program Files-ке орнатса да жұмыс істейді)
      dataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ONAIR");
      try { Directory.CreateDirectory(dataDir); } catch {}
      // Ескі портативті деректерді жаңа қалтаға бір рет көшіру
      foreach (string n in new string[] { "settings.txt", "android-device.txt", "phones.txt" }) {
        try { string o = Path.Combine(baseDir, n), nw = Path.Combine(dataDir, n); if (File.Exists(o) && !File.Exists(nw)) File.Copy(o, nw); } catch {}
      }
      deviceFile = Path.Combine(dataDir, "android-device.txt");
      recDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "ONAIR");
      LoadSettings(); LoadPhones();
      this.TopMost = topMost;

      string bin = Path.Combine(baseDir, "bin");
      string plugins = Path.Combine(baseDir, "lib", "gstreamer-1.0");
      string scanner = Path.Combine(baseDir, "libexec", "gstreamer-1.0", "gst-plugin-scanner.exe");
      Environment.SetEnvironmentVariable("PATH", bin + ";" + Environment.GetEnvironmentVariable("PATH"));
      Environment.SetEnvironmentVariable("GST_PLUGIN_SYSTEM_PATH_1_0", plugins);
      Environment.SetEnvironmentVariable("GST_PLUGIN_SYSTEM_PATH", plugins);
      Environment.SetEnvironmentVariable("GST_PLUGIN_SCANNER_1_0", scanner);
      Environment.SetEnvironmentVariable("GST_PLUGIN_SCANNER", scanner);

      try { logoBmp = new Bitmap(Path.Combine(baseDir, "logo.png")); } catch {}
      fDisplay = new Font("Segoe UI Semibold", 14, FontStyle.Bold);
      fWord    = new Font("Segoe UI Black", 15, FontStyle.Bold);
      fBtn     = new Font("Segoe UI Semibold", 10, FontStyle.Bold);
      fHint    = new Font("Segoe UI", 10.5f);
      fEyebrow = new Font("Consolas", 9.5f, FontStyle.Bold);
      fMono    = new Font("Consolas", 11);

      Text = "ONAIR";
      ClientSize = new Size(470, 650);
      MinimumSize = new Size(430, 360);
      StartPosition = FormStartPosition.CenterScreen;
      FormBorderStyle = FormBorderStyle.Sizable;
      BackColor = Brand.Bg;
      Font = new Font("Segoe UI", 9);
      try { string ico = Path.Combine(baseDir, "onair.ico"); if (File.Exists(ico)) Icon = new Icon(ico); } catch {}

      bar = new Panel(); bar.Dock = DockStyle.Top; bar.Height = 66; bar.BackColor = Brand.Bar;
      bar.Paint += BarPaint;
      bar.MouseClick += delegate (object sm, MouseEventArgs em) { if (gearRect.Contains(em.Location)) OpenSettings(); };
      bar.MouseMove += delegate (object sm, MouseEventArgs em) {
        bool hv = gearRect.Contains(em.Location);
        if (hv != gearHot) { gearHot = hv; bar.Cursor = hv ? Cursors.Hand : Cursors.Default; bar.Invalidate(); }
      };
      Controls.Add(bar);

      btnAdd = new RoundButton();
      btnAdd.Shape = RoundButton.Ico.Plus;
      btnAdd.Style = RoundButton.Mode.Ghost; btnAdd.Radius = 19; btnAdd.Border = Brand.Hair;
      btnAdd.Size = new Size(38, 38); btnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      btnAdd.Location = new Point(bar.Width - 266, 14);
      btnAdd.BackColor = Brand.Surf; btnAdd.ForeColor = Brand.Fg; btnAdd.Cursor = Cursors.Hand;
      btnAdd.Click += delegate { AddPhone(); };
      ToolTip tt = new ToolTip(); tt.SetToolTip(btnAdd, "Жаңа Android телефонды сымсыз қосу");
      bar.Controls.Add(btnAdd);

      btnIos = MakePill("▶  iPhone", 214);
      btnIos.Click += delegate { ToggleIos(); };
      bar.Controls.Add(btnIos);

      btnAndroid = MakePill("▶  Android", 108);
      btnAndroid.Click += delegate { ToggleAndroid(); };
      bar.Controls.Add(btnAndroid);

      // ── Төменгі басқару жолағы (эфирде көрінеді) ──
      tools = new Panel(); tools.Dock = DockStyle.Bottom; tools.Height = 48; tools.BackColor = Brand.Bar; tools.Visible = false;
      tools.Paint += delegate (object s2, PaintEventArgs pe2) {
        using (Pen pn = new Pen(Brand.Hair, 1)) pe2.Graphics.DrawLine(pn, 0, 0, tools.Width, 0);
      };
      Controls.Add(tools);
      btnShot   = MakeToolPill("Сурет", 92);        btnShot.Click   += delegate { Screenshot(); };
      btnRotate = MakeToolPill("Бұру", 82);         btnRotate.Click += delegate { ToggleLandscape(); };
      btnFull   = MakeToolPill("Толық экран", 132); btnFull.Click   += delegate { ToggleFullscreen(); };
      btnRec = new RoundButton();
      btnRec.Shape = RoundButton.Ico.Dot; btnRec.Style = RoundButton.Mode.Ghost; btnRec.Radius = 17; btnRec.Border = Brand.Hair;
      btnRec.Size = new Size(36, 34); btnRec.BackColor = Brand.Surf; btnRec.ForeColor = Brand.Live; btnRec.Cursor = Cursors.Hand;
      btnRec.Click += delegate { ToggleRecord(); };
      tt.SetToolTip(btnRec, "Жазу / Жазуды тоқтату (.mp4)");
      tools.Controls.Add(btnShot); tools.Controls.Add(btnRotate); tools.Controls.Add(btnFull); tools.Controls.Add(btnRec);
      // Android навигация (2-қатарда, Android эфирде көрінеді)
      btnBack    = MakeNav(RoundButton.Ico.TriLeft, 4);   tt.SetToolTip(btnBack, "Артқа");
      btnHome    = MakeNav(RoundButton.Ico.Circle, 3);     tt.SetToolTip(btnHome, "Үй");
      btnRecents = MakeNav(RoundButton.Ico.SquareO, 187);  tt.SetToolTip(btnRecents, "Терезелер");
      btnPower   = MakeNav(RoundButton.Ico.Power, 26);      tt.SetToolTip(btnPower, "Қуат");
      btnVolDn   = MakeNav(RoundButton.Ico.Minus, 25);      tt.SetToolTip(btnVolDn, "Дыбыс −");
      btnVolUp   = MakeNav(RoundButton.Ico.Plus, 24);       tt.SetToolTip(btnVolUp, "Дыбыс +");
      btnInfo = new RoundButton();
      btnInfo.Shape = RoundButton.Ico.Info; btnInfo.Style = RoundButton.Mode.Ghost; btnInfo.Radius = 16; btnInfo.Border = Brand.Hair;
      btnInfo.Size = new Size(34, 34); btnInfo.BackColor = Brand.Surf; btnInfo.ForeColor = Brand.Fg; btnInfo.Cursor = Cursors.Hand;
      btnInfo.Visible = false; btnInfo.Click += delegate { ShowPhoneInfo(); };
      tt.SetToolTip(btnInfo, "Телефон туралы");
      tools.Controls.Add(btnBack); tools.Controls.Add(btnHome); tools.Controls.Add(btnRecents);
      tools.Controls.Add(btnPower); tools.Controls.Add(btnVolDn); tools.Controls.Add(btnVolUp); tools.Controls.Add(btnInfo);
      // Аннотация (сурет салу) басқару элементтері
      btnAnnot = MakeToolPill("Сурет салу", 110); btnAnnot.Click += delegate { ToggleAnnot(); };
      tools.Controls.Add(btnAnnot);
      colorBtns = new RoundButton[palette.Length];
      for (int ci = 0; ci < palette.Length; ci++) {
        RoundButton cb = new RoundButton();
        cb.Style = RoundButton.Mode.Solid; cb.Radius = 16; cb.Size = new Size(32, 32);
        cb.BackColor = palette[ci]; cb.Cursor = Cursors.Hand; cb.Visible = false;
        int cc = ci; cb.Click += delegate { curColor = cc; };
        colorBtns[ci] = cb; tools.Controls.Add(cb);
      }
      btnClear = MakeToolPill("Тазалау", 100); btnClear.Visible = false;
      btnDone  = MakeToolPill("Дайын", 92);    btnDone.Visible = false;
      tools.Controls.Add(btnClear); tools.Controls.Add(btnDone);
      tools.SizeChanged += delegate { LayoutTools(); };
      shotFlash = new Timer(); shotFlash.Interval = 1200;
      shotFlash.Tick += delegate { shotFlash.Stop(); btnShot.Text = "Сурет"; btnShot.ForeColor = Brand.Fg; btnShot.Invalidate(); };

      video = new DBPanel(); video.Dock = DockStyle.Fill; video.BackColor = Brand.Bg1;
      video.Paint += VideoPaint;
      Controls.Add(video); video.BringToFront();
      video.SizeChanged += delegate { LayoutChildren(); video.Invalidate(); if (annotating) PositionOverlay(false); };

      timer = new Timer(); timer.Interval = 240; timer.Tick += OnTick; timer.Start();
      RefreshLang(); ApplyTheme();

      this.Shown += delegate { try { int v = light ? 0 : 1; DwmSetWindowAttribute(this.Handle, 20, ref v, 4); } catch {} };
      this.FormClosing += delegate { timer.Stop(); StopIos(); StopAndroid(); };
      this.KeyPreview = true;
      this.KeyDown += delegate (object sk, KeyEventArgs ek) { if (ek.KeyCode == Keys.Escape && (fullscreen || annotating)) { if (annotating) ToggleAnnot(); else ToggleFullscreen(); } };
      this.LocationChanged += delegate { if (annotating) PositionOverlay(false); };

      if (Environment.GetEnvironmentVariable("ONAIR_SELFTEST") == "1") {
        Timer t2 = new Timer(); t2.Interval = 1500;
        t2.Tick += delegate { t2.Stop(); this.Close(); }; t2.Start();
      }
      if (Environment.GetEnvironmentVariable("ONAIR_TESTDIALOG") == "1") {
        this.Shown += delegate { BeginInvoke(new Action(delegate { AddPhone(); })); };
      }
      if (Environment.GetEnvironmentVariable("ONAIR_TESTLIVE") == "1") {
        this.Shown += delegate { BeginInvoke(new Action(delegate { StartAndroid(); })); };
      }
      if (Environment.GetEnvironmentVariable("ONAIR_TESTSETTINGS") == "1") {
        this.Shown += delegate { BeginInvoke(new Action(delegate { OpenSettings(); })); };
      }
      if (Environment.GetEnvironmentVariable("ONAIR_TESTANNOT") == "1") {
        this.Shown += delegate { BeginInvoke(new Action(delegate { StartAndroid(); })); };
        Timer t3 = new Timer(); t3.Interval = 8000;
        t3.Tick += delegate { t3.Stop(); if (embAndroid && !annotating) ToggleAnnot(); }; t3.Start();
      }
      if (Environment.GetEnvironmentVariable("ONAIR_TESTREC") == "1") {
        // Жазуды дереккөзсіз тексеру: панельді 5 сек жазып, файлды тексеру үшін жабамыз
        this.Shown += delegate {
          Timer ta = new Timer(); ta.Interval = 1500;
          ta.Tick += delegate {
            ta.Stop();
            try { Directory.CreateDirectory(recDir); } catch {}
            recPath = Path.Combine(recDir, "TESTREC.mp4");
            recProc = SpawnCapture(recPath, video.RectangleToScreen(video.ClientRectangle));
            Timer tb = new Timer(); tb.Interval = 5000;
            tb.Tick += delegate { tb.Stop(); try { if (recProc != null && !recProc.HasExited) recProc.Kill(); } catch {} this.Close(); }; tb.Start();
          };
          ta.Start();
        };
      }
    }

    RoundButton MakePill(string text, int rightOffset) {
      RoundButton b = new RoundButton();
      b.Text = text; b.Font = fBtn; b.Style = RoundButton.Mode.Ghost; b.Radius = 19;
      b.Size = new Size(100, 38); b.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      b.Location = new Point(bar.Width - rightOffset, 14);
      b.BackColor = Brand.Surf; b.Border = Brand.Hair; b.ForeColor = Brand.Fg; b.Cursor = Cursors.Hand;
      return b;
    }

    RoundButton MakeToolPill(string text, int w) {
      RoundButton b = new RoundButton();
      b.Text = text; b.Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold);
      b.Style = RoundButton.Mode.Ghost; b.Radius = 17; b.Border = Brand.Hair;
      b.Size = new Size(w, 34);
      b.BackColor = Brand.Surf; b.ForeColor = Brand.Fg; b.Cursor = Cursors.Hand;
      return b;
    }

    RoundButton MakeNav(RoundButton.Ico shape, int keycode) {
      RoundButton b = new RoundButton();
      b.Shape = shape; b.Style = RoundButton.Mode.Ghost; b.Radius = 16; b.Border = Brand.Hair;
      b.Size = new Size(34, 34); b.BackColor = Brand.Surf; b.ForeColor = Brand.Fg; b.Cursor = Cursors.Hand;
      b.Visible = false;
      b.Click += delegate { NavKey(keycode); };
      return b;
    }
    void NavKey(int code) {
      string serial = GetSerial();
      if (serial.Length == 0) return;
      try { RunAdb("-s " + serial + " shell input keyevent " + code); } catch {}
    }

    int ParseBattery(string dump) {
      foreach (string line in dump.Split('\n')) {
        string t = line.Trim();
        if (t.StartsWith("level:")) { int n; if (int.TryParse(t.Substring(6).Trim(), out n)) return n; }
      }
      return -1;
    }
    void ShowPhoneInfo() {
      string serial = GetSerial();
      if (serial.Length == 0) return;
      string model = RunAdb("-s " + serial + " shell getprop ro.product.model").Trim();
      string brand = RunAdb("-s " + serial + " shell getprop ro.product.brand").Trim();
      string ver   = RunAdb("-s " + serial + " shell getprop ro.build.version.release").Trim();
      int batt = ParseBattery(RunAdb("-s " + serial + " shell dumpsys battery"));
      string ip = serial.Contains(":") ? serial.Split(':')[0] : serial;

      Form d = new Form();
      d.Text = S("phone_info"); d.ClientSize = new Size(380, 282);
      d.StartPosition = FormStartPosition.CenterParent; d.FormBorderStyle = FormBorderStyle.FixedDialog;
      d.MaximizeBox = false; d.MinimizeBox = false; d.BackColor = Brand.Bg; d.Font = new Font("Segoe UI", 9);
      try { d.Icon = this.Icon; } catch {}
      d.Shown += delegate { try { int v = light ? 0 : 1; DwmSetWindowAttribute(d.Handle, 20, ref v, 4); } catch {} };
      Label title = new Label(); title.Text = S("phone_info"); title.Font = new Font("Segoe UI Semibold", 15, FontStyle.Bold);
      title.ForeColor = Brand.Fg; title.AutoSize = true; title.Location = new Point(22, 18); d.Controls.Add(title);

      RoundPanel card = new RoundPanel(); card.Location = new Point(22, 58); card.Size = new Size(336, 146);
      card.Fill = Brand.Surf; card.Border = Brand.Hair; card.Radius = 16; d.Controls.Add(card);
      string[] labels = { S("model"), "Android", S("battery"), "IP" };
      string[] values = { (brand + " " + model).Trim(), (ver.Length > 0 ? ver : "—"), (batt >= 0 ? batt + "%" : "—"), ip };
      int ry = 16;
      for (int i = 0; i < 4; i++) {
        Label l = new Label(); l.Text = labels[i]; l.ForeColor = Brand.FgMute; l.Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold);
        l.AutoSize = false; l.Size = new Size(108, 24); l.Location = new Point(16, ry); l.BackColor = Color.Transparent; card.Controls.Add(l);
        Label vv = new Label(); vv.Text = values[i]; vv.ForeColor = Brand.Fg; vv.Font = new Font("Segoe UI Semibold", 10.5f, FontStyle.Bold);
        vv.AutoSize = false; vv.Size = new Size(204, 24); vv.Location = new Point(124, ry); vv.BackColor = Color.Transparent; card.Controls.Add(vv);
        ry += 32;
      }
      RoundButton close = new RoundButton(); close.Text = S("close"); close.Font = new Font("Segoe UI Semibold", 10.5f, FontStyle.Bold);
      close.Style = RoundButton.Mode.Gradient; close.Radius = 20; close.Size = new Size(180, 40); close.Location = new Point((380 - 180) / 2, 220);
      close.ForeColor = Color.White; close.Cursor = Cursors.Hand; close.Click += delegate { d.Close(); }; d.Controls.Add(close);
      d.ShowDialog(this);
    }

    void LayoutTools() {
      if (tools == null || btnRec == null || btnVolUp == null || btnDone == null) return;
      int gap = 8;
      if (annotating) {   // ── Сурет салу режимі: түстер + Тазалау + Дайын ──
        btnShot.Visible = btnRotate.Visible = btnFull.Visible = btnAnnot.Visible = btnRec.Visible = false;
        btnBack.Visible = btnHome.Visible = btnRecents.Visible = btnPower.Visible = btnVolDn.Visible = btnVolUp.Visible = btnInfo.Visible = false;
        foreach (RoundButton cb in colorBtns) cb.Visible = true;
        btnClear.Visible = true; btnDone.Visible = true;
        if (tools.Height != 48) tools.Height = 48;
        int total = colorBtns.Length * 32 + btnClear.Width + btnDone.Width + gap * (colorBtns.Length + 1) + 10;
        int x = (tools.Width - total) / 2; if (x < 8) x = 8; int y = (tools.Height - 32) / 2;
        foreach (RoundButton cb in colorBtns) { cb.Location = new Point(x, y); x += 32 + gap; }
        x += 6;
        btnClear.Location = new Point(x, (tools.Height - 34) / 2); x += btnClear.Width + gap;
        btnDone.Location  = new Point(x, (tools.Height - 34) / 2);
        return;
      }
      foreach (RoundButton cb in colorBtns) cb.Visible = false; btnClear.Visible = false; btnDone.Visible = false;
      bool navOn = AndroidRunning;
      btnShot.Visible = btnRotate.Visible = btnFull.Visible = btnAnnot.Visible = btnRec.Visible = true;
      btnBack.Visible = btnHome.Visible = btnRecents.Visible = btnPower.Visible = btnVolDn.Visible = btnVolUp.Visible = btnInfo.Visible = navOn;
      int wantH = navOn ? 92 : 48;
      if (tools.Height != wantH) tools.Height = wantH;
      // 1-қатар: Сурет/Бұру/Толық/Сурет салу/●
      int total1 = btnShot.Width + btnRotate.Width + btnFull.Width + btnAnnot.Width + btnRec.Width + gap * 4;
      int xx = (tools.Width - total1) / 2; if (xx < 8) xx = 8;
      int y1 = navOn ? 8 : (tools.Height - 34) / 2;
      btnShot.Location   = new Point(xx, y1); xx += btnShot.Width + gap;
      btnRotate.Location = new Point(xx, y1); xx += btnRotate.Width + gap;
      btnFull.Location   = new Point(xx, y1); xx += btnFull.Width + gap;
      btnAnnot.Location  = new Point(xx, y1); xx += btnAnnot.Width + gap;
      btnRec.Location    = new Point(xx, y1);
      // 2-қатар: Android навигация
      if (navOn) {
        int nav = 34, ng = 8;
        RoundButton[] nb = { btnBack, btnHome, btnRecents, btnPower, btnVolDn, btnVolUp, btnInfo };
        int total2 = nb.Length * nav + (nb.Length - 1) * ng;
        int nx = (tools.Width - total2) / 2; if (nx < 8) nx = 8;
        foreach (RoundButton b in nb) { b.Location = new Point(nx, 50); nx += nav + ng; }
      }
    }

    // ── 📸 Скриншот ──────────────────────────────────────────
    void Screenshot() {
      if (!embIos && !embAndroid) return;
      try { Directory.CreateDirectory(recDir); } catch {}
      string ts = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
      int n = 0;
      if (embIos && childIos != IntPtr.Zero)         n += CaptureWindow(childIos, Path.Combine(recDir, "iPhone_сурет_" + ts + ".png"));
      if (embAndroid && childAndroid != IntPtr.Zero) n += CaptureWindow(childAndroid, Path.Combine(recDir, "Android_сурет_" + ts + ".png"));
      if (n > 0) { btnShot.Text = "Сақталды ✓"; btnShot.ForeColor = Brand.Mint; btnShot.Invalidate(); shotFlash.Stop(); shotFlash.Start(); }
    }
    int CaptureWindow(IntPtr h, string path) {
      try {
        RECT r; GetWindowRect(h, out r);
        int w = r.Right - r.Left, ht = r.Bottom - r.Top;
        if (w <= 0 || ht <= 0) return 0;
        using (Bitmap bmp = new Bitmap(w, ht))
        using (Graphics g = Graphics.FromImage(bmp)) {
          g.CopyFromScreen(r.Left, r.Top, 0, 0, new Size(w, ht));
          bmp.Save(path, System.Drawing.Imaging.ImageFormat.Png);
        }
        return 1;
      } catch { return 0; }
    }

    // ── 🔄 Көлденең (альбом) бағдар — Android ──────────────────
    void ToggleLandscape() {
      androidLandscape = !androidLandscape;
      btnRotate.Style = androidLandscape ? RoundButton.Mode.Gradient : RoundButton.Mode.Ghost;
      btnRotate.ForeColor = Color.White; btnRotate.Invalidate();
      if (AndroidRunning) StartAndroid(); // жаңа бағдармен қайта қосу
    }

    // ── ✏️ Экранға сурет салу (кадрды түсіріп, үстіне сызу) ──
    void ToggleAnnot() { OpenAnnotEditor(); }
    void PositionOverlay(bool forceInit) { }
    void OpenAnnotEditor() {
      if (!embIos && !embAndroid) return;
      Bitmap shot = CaptureVideo();
      if (shot == null) return;
      AnnotEditor ed = new AnnotEditor(shot, palette, recDir, light, S("clear"), S("save"), S("close"));
      try { ed.Icon = this.Icon; } catch {}
      ed.ShowDialog(this);
    }
    Bitmap CaptureVideo() {
      try {
        Rectangle r = video.RectangleToScreen(video.ClientRectangle);
        if (r.Width < 1 || r.Height < 1) return null;
        Bitmap b = new Bitmap(r.Width, r.Height);
        using (Graphics g = Graphics.FromImage(b)) g.CopyFromScreen(r.Location, Point.Empty, r.Size);
        return b;
      } catch { return null; }
    }

    // ── ⛶ Толық экран ────────────────────────────────────────
    void ToggleFullscreen() {
      if (annotating) ToggleAnnot();
      if (!fullscreen) {
        savedBounds = this.Bounds; savedBorder = this.FormBorderStyle;
        this.FormBorderStyle = FormBorderStyle.None;
        bar.Visible = false; tools.Visible = false;
        this.Bounds = Screen.FromControl(this).Bounds;
        fullscreen = true; LayoutChildren();
      } else {
        this.FormBorderStyle = savedBorder;
        bar.Visible = true; tools.Visible = (embIos || embAndroid);
        this.Bounds = savedBounds;
        fullscreen = false; LayoutTools(); LayoutChildren();
      }
    }

    // ===================== Тіл (i18n) =====================
    string P(string kz, string ru, string en) { return (lang == "ru") ? ru : (lang == "en" ? en : kz); }
    string S(string k) {
      switch (k) {
        case "shot":     return P("Сурет", "Снимок", "Snapshot");
        case "rotate":   return P("Бұру", "Поворот", "Rotate");
        case "full":     return P("Толық", "Экран", "Full");
        case "settings": return P("Баптаулар", "Настройки", "Settings");
        case "idle_t":   return P("Телефон экранын осында", "Экран телефона здесь", "Your phone screen here");
        case "hint":     return P(
          "▶ iPhone   немесе   ▶ Android   басыңыз\n\nЕкеуін қатар да қосуға болады.\n\niPhone:  Screen Mirroring → ONAIR\nAndroid:  «＋» арқылы бір рет жұптаңыз",
          "Нажмите   ▶ iPhone   или   ▶ Android\n\nМожно подключить оба сразу.\n\niPhone:  Screen Mirroring → ONAIR\nAndroid:  спарьте через «＋»",
          "Press   ▶ iPhone   or   ▶ Android\n\nYou can connect both at once.\n\niPhone:  Screen Mirroring → ONAIR\nAndroid:  pair once via «＋»");
        case "lang":     return P("Тіл", "Язык", "Language");
        case "resolution": return P("Android айқындық", "Разрешение Android", "Android resolution");
        case "afps":     return P("Android кадр/сек", "Android кадры/сек", "Android frame rate");
        case "bitrate":  return P("Сапа (битрейт)", "Битрейт", "Bitrate");
        case "ifps":     return P("iPhone кадр/сек", "iPhone кадры/сек", "iPhone frame rate");
        case "auto":     return P("Авто", "Авто", "Auto");
        case "high":     return P("Жоғары", "Высокий", "High");
        case "mid":      return P("Орта", "Средний", "Medium");
        case "low":      return P("Төмен", "Низкий", "Low");
        case "close":    return P("Жабу", "Закрыть", "Close");
        case "topmost":  return P("Әрқашан үстінде", "Поверх окон", "Always on top");
        case "screenoff":return P("Экран өшіулі (Android)", "Экран выкл (Android)", "Screen off (Android)");
        case "micrec":   return P("Микрофон (жазуда)", "Микрофон (запись)", "Microphone (recording)");
        case "yes":      return P("Иә", "Да", "Yes");
        case "no":       return P("Жоқ", "Нет", "No");
        case "theme":    return P("Тема", "Тема", "Theme");
        case "dark":     return P("Қараңғы", "Тёмная", "Dark");
        case "light":    return P("Ашық", "Светлая", "Light");
        case "phone_info": return P("Телефон туралы", "О телефоне", "Phone info");
        case "model":    return P("Модель", "Модель", "Model");
        case "battery":  return P("Батарея", "Батарея", "Battery");
        case "annotate": return P("Сурет салу", "Рисовать", "Draw");
        case "clear":    return P("Тазалау", "Очистить", "Clear");
        case "done":     return P("Дайын", "Готово", "Done");
        case "save":     return P("Сақтау", "Сохранить", "Save");
        case "pick_phone": return P("Телефонды таңдаңыз", "Выберите телефон", "Choose phone");
        case "add_title": return P("Жаңа Android телефонды қосу", "Подключить новый Android", "Add a new Android phone");
        case "add_steps": return P(
          "Телефонда (бір рет):\r\n1)  About phone → «Build number» 7 рет басу\r\n2)  Developer options → «Wireless debugging» ҚОСУ\r\n3)  «Wireless debugging» жазуын басу:\r\n      •  «Pair device…» = IP, ЖҰПТАУ ПОРТЫ, PIN\r\n      •  «IP address & Port» (негізгі) = ҚОСЫЛУ ПОРТЫ",
          "На телефоне (один раз):\r\n1)  About phone → «Build number» нажать 7 раз\r\n2)  Developer options → включить «Wireless debugging»\r\n3)  Нажать на «Wireless debugging»:\r\n      •  «Pair device…» = IP, ПОРТ СОПРЯЖЕНИЯ, PIN\r\n      •  «IP address & Port» (главный) = ПОРТ ПОДКЛЮЧЕНИЯ",
          "On the phone (once):\r\n1)  About phone → tap «Build number» 7 times\r\n2)  Developer options → enable «Wireless debugging»\r\n3)  Tap «Wireless debugging»:\r\n      •  «Pair device…» = IP, PAIRING PORT, PIN\r\n      •  «IP address & Port» (main) = CONNECT PORT");
        case "f_ip":   return P("IP мекенжайы", "IP адрес", "IP address");
        case "f_pair": return P("Жұптау порты", "Порт сопряжения", "Pairing port");
        case "f_pin":  return P("PIN коды", "PIN код", "PIN code");
        case "f_conn": return P("Қосылу порты", "Порт подключения", "Connect port");
        case "pair_save": return P("Жұптау және сақтау", "Спарить и сохранить", "Pair & save");
        case "cancel": return P("Болдырмау", "Отмена", "Cancel");
        case "fill_all": return P("Барлық 4 өрісті толтырыңыз.", "Заполните все 4 поля.", "Fill in all 4 fields.");
        case "pairing": return P("Жұпталуда… күте тұрыңыз.", "Сопряжение… подождите.", "Pairing… please wait.");
        case "pair_fail": return P("Жұптау сәтсіз. PIN/Жұптау порты дұрыс па, Pair терезесі ашық па?", "Сопряжение не удалось. Верны ли PIN/порт и открыто ли окно Pair?", "Pairing failed. Is the PIN/port correct and the Pair dialog open?");
        case "paired_conn": return P("Жұпталды. Қосылуда…", "Спарено. Подключение…", "Paired. Connecting…");
        case "conn_fail": return P("Жұпталды, бірақ қосыла алмады. «Қосылу порты» дұрыс па?", "Спарено, но не подключается. Верен ли «порт подключения»?", "Paired but can't connect. Is the «connect port» correct?");
        case "saved_ok": return P("Сәтті! Телефон сақталды.", "Готово! Телефон сохранён.", "Done! Phone saved.");
        case "save_msg": return P("Телефон сәтті жұпталды және сақталды!\r\n\r\nЕнді «▶ Android» бассаңыз — бірден қосылады.", "Телефон успешно спарен и сохранён!\r\n\r\nТеперь нажмите «▶ Android» — подключится сразу.", "Phone paired and saved!\r\n\r\nNow press «▶ Android» — it connects instantly.");
        case "not_conn": return P("Телефон қосылмады.\r\n\r\nWireless debugging ҚОСУЛЫ ма?\r\nЖаңа телефон болса — «＋» арқылы жұптаңыз.\r\n\r\n«IP address & Port» санын енгізіңіз:", "Телефон не подключён.\r\n\r\nВключён ли Wireless debugging?\r\nНовый телефон — спарьте через «＋».\r\n\r\nВведите «IP address & Port»:", "Phone not connected.\r\n\r\nIs Wireless debugging enabled?\r\nNew phone — pair via «＋».\r\n\r\nEnter «IP address & Port»:");
        case "conn_fail2": return P("Қосыла алмады. Тексеріңіз:\r\n • Wireless debugging ҚОСУЛЫ\r\n • бір Wi-Fi-да\r\n • IP:PORT дұрыс", "Не удалось подключиться. Проверьте:\r\n • Wireless debugging включён\r\n • одна сеть Wi-Fi\r\n • IP:PORT верный", "Couldn't connect. Check:\r\n • Wireless debugging is on\r\n • same Wi-Fi\r\n • IP:PORT is correct");
        case "ios_rec": return P("iPhone жазу басталды.\r\n\r\nAirPlay байланысы үзілді — iPhone-да:\r\nScreen Mirroring → ONAIR-ды ҚАЙТА таңдаңыз.", "Запись iPhone началась.\r\n\r\nAirPlay прервался — на iPhone:\r\nScreen Mirroring → выберите ONAIR ЗАНОВО.", "iPhone recording started.\r\n\r\nAirPlay dropped — on iPhone:\r\nScreen Mirroring → select ONAIR AGAIN.");
      }
      return k;
    }

    // ===================== Баптауларды сақтау =====================
    void LoadSettings() {
      try {
        string f = Path.Combine(dataDir, "settings.txt");
        if (!File.Exists(f)) return;
        foreach (string line in File.ReadAllLines(f)) {
          int eq = line.IndexOf('='); if (eq < 1) continue;
          string k = line.Substring(0, eq).Trim(), v = line.Substring(eq + 1).Trim();
          if (k == "lang") lang = v;
          else if (k == "amax") int.TryParse(v, out aMaxSize);
          else if (k == "afps") int.TryParse(v, out aMaxFps);
          else if (k == "abr")  int.TryParse(v, out aBitRate);
          else if (k == "ifps") int.TryParse(v, out iosFps);
          else if (k == "top")  topMost = (v == "1");
          else if (k == "soff") screenOff = (v == "1");
          else if (k == "theme") light = (v == "1");
          else if (k == "mic") recMic = (v == "1");
        }
      } catch {}
    }
    void SaveSettings() {
      try {
        File.WriteAllText(Path.Combine(dataDir, "settings.txt"),
          "lang=" + lang + "\r\namax=" + aMaxSize + "\r\nafps=" + aMaxFps + "\r\nabr=" + aBitRate +
          "\r\nifps=" + iosFps + "\r\ntop=" + (topMost ? "1" : "0") + "\r\nsoff=" + (screenOff ? "1" : "0") +
          "\r\ntheme=" + (light ? "1" : "0") + "\r\nmic=" + (recMic ? "1" : "0") + "\r\n");
      } catch {}
    }

    // ===================== Ашық/Қараңғы тема =====================
    void ApplyTheme() {
      if (light) {
        Brand.Bg = Color.FromArgb(238, 239, 243); Brand.Bg1 = Color.FromArgb(255, 255, 255); Brand.Bar = Color.FromArgb(255, 255, 255);
        Brand.Surf = Color.FromArgb(243, 244, 248); Brand.Surf2 = Color.FromArgb(232, 233, 240);
        Brand.Hair = Color.FromArgb(220, 221, 230); Brand.Hair2 = Color.FromArgb(198, 200, 212);
        Brand.Fg = Color.FromArgb(22, 23, 28); Brand.FgMute = Color.FromArgb(92, 94, 107); Brand.FgFaint = Color.FromArgb(154, 156, 168);
      } else {
        Brand.Bg = Color.FromArgb(7, 7, 10); Brand.Bg1 = Color.FromArgb(12, 12, 17); Brand.Bar = Color.FromArgb(11, 11, 15);
        Brand.Surf = Color.FromArgb(24, 24, 30); Brand.Surf2 = Color.FromArgb(31, 31, 39);
        Brand.Hair = Color.FromArgb(38, 38, 48); Brand.Hair2 = Color.FromArgb(54, 54, 66);
        Brand.Fg = Color.FromArgb(244, 244, 247); Brand.FgMute = Color.FromArgb(167, 167, 180); Brand.FgFaint = Color.FromArgb(106, 106, 120);
      }
      this.BackColor = Brand.Bg; bar.BackColor = Brand.Bar; tools.BackColor = Brand.Bar; video.BackColor = Brand.Bg1;
      RoundButton[] ghosts = { btnAdd, btnShot, btnRotate, btnFull, btnBack, btnHome, btnRecents, btnPower, btnVolDn, btnVolUp, btnInfo };
      foreach (RoundButton b in ghosts) { if (b == null) continue; b.BackColor = Brand.Surf; b.Border = Brand.Hair; b.ForeColor = Brand.Fg; b.Invalidate(); }
      if (androidLandscape) { btnRotate.Style = RoundButton.Mode.Gradient; btnRotate.ForeColor = Color.White; }
      UpdateUi();
      try { int v = light ? 0 : 1; DwmSetWindowAttribute(this.Handle, 20, ref v, 4); } catch {}
      this.Invalidate(true); bar.Invalidate(); tools.Invalidate(); video.Invalidate();
    }

    // ===================== Бірнеше телефон =====================
    void LoadPhones() {
      try {
        string f = Path.Combine(dataDir, "phones.txt");
        if (File.Exists(f))
          foreach (string line in File.ReadAllLines(f)) { string t = line.Trim(); if (t.Length > 0 && !phones.Contains(t)) phones.Add(t); }
        string sv = LoadDevice();
        if (sv.Length > 0 && !phones.Contains(sv)) phones.Add(sv);
      } catch {}
    }
    void SavePhones() { try { File.WriteAllText(Path.Combine(dataDir, "phones.txt"), string.Join("\r\n", phones.ToArray())); } catch {} }
    void AddPhoneToList(string dev) {
      if (dev.Length == 0) return;
      if (!phones.Contains(dev)) { phones.Add(dev); SavePhones(); }
      SaveDevice(dev);
    }

    string PickPhone() {
      Form d = new Form();
      d.Text = S("pick_phone"); d.ClientSize = new Size(380, 76 + phones.Count * 48);
      d.StartPosition = FormStartPosition.CenterParent; d.FormBorderStyle = FormBorderStyle.FixedDialog;
      d.MaximizeBox = false; d.MinimizeBox = false; d.BackColor = Brand.Bg; d.Font = new Font("Segoe UI", 9);
      try { d.Icon = this.Icon; } catch {}
      d.Shown += delegate { try { int v = light ? 0 : 1; DwmSetWindowAttribute(d.Handle, 20, ref v, 4); } catch {} };
      Label title = new Label(); title.Text = S("pick_phone"); title.Font = new Font("Segoe UI Semibold", 13, FontStyle.Bold);
      title.ForeColor = Brand.Fg; title.AutoSize = true; title.Location = new Point(20, 16); d.Controls.Add(title);
      string[] result = { null };
      int yy = 52;
      for (int i = 0; i < phones.Count; i++) {
        string ph = phones[i];
        RoundButton b = new RoundButton();
        b.Text = ph; b.Font = new Font("Consolas", 11, FontStyle.Bold);
        b.Style = RoundButton.Mode.Ghost; b.Radius = 18; b.Border = Brand.Hair;
        b.Size = new Size(340, 40); b.Location = new Point(20, yy);
        b.BackColor = Brand.Surf; b.ForeColor = Brand.Fg; b.Cursor = Cursors.Hand;
        b.Click += delegate { result[0] = ph; d.Close(); };
        d.Controls.Add(b); yy += 48;
      }
      d.ShowDialog(this);
      return result[0];
    }

    void RefreshLang() {
      SizePill(btnShot, S("shot")); SizePill(btnRotate, S("rotate")); SizePill(btnFull, S("full"));
      SizePill(btnAnnot, S("annotate")); SizePill(btnClear, S("clear")); SizePill(btnDone, S("done"));
      LayoutTools(); video.Invalidate();
    }
    void SizePill(RoundButton b, string text) {
      b.Text = text;
      Size m = TextRenderer.MeasureText(text, b.Font);
      b.Width = Math.Max(72, m.Width + 28);
    }

    // ===================== Баптаулар терезесі =====================
    void OpenSettings() {
      Form d = new Form();
      d.Text = S("settings"); d.ClientSize = new Size(450, 852);
      d.StartPosition = FormStartPosition.CenterParent; d.FormBorderStyle = FormBorderStyle.FixedDialog;
      d.MaximizeBox = false; d.MinimizeBox = false; d.BackColor = Brand.Bg; d.Font = new Font("Segoe UI", 9);
      try { d.Icon = this.Icon; } catch {}
      d.Shown += delegate { try { int v = light ? 0 : 1; DwmSetWindowAttribute(d.Handle, 20, ref v, 4); } catch {} };

      Label title = new Label(); title.Font = new Font("Segoe UI Semibold", 15, FontStyle.Bold);
      title.ForeColor = Brand.Fg; title.AutoSize = true; title.Location = new Point(22, 18); title.Text = S("settings"); d.Controls.Add(title);

      int y = 62;
      Seg(d, ref y, S("lang"), new string[] { "Қазақша", "Русский", "English" }, new string[] { "kz", "ru", "en" }, lang,
        delegate (string v) { lang = v; SaveSettings(); RefreshLang(); title.Text = S("settings"); d.Text = S("settings"); });
      Seg(d, ref y, S("theme"), new string[] { S("dark"), S("light") }, new string[] { "0", "1" }, light ? "1" : "0",
        delegate (string v) { light = (v == "1"); SaveSettings(); ApplyTheme(); });
      Seg(d, ref y, S("resolution"), new string[] { S("auto"), "1080", "720", "480" }, new string[] { "0", "1080", "720", "480" }, aMaxSize.ToString(),
        delegate (string v) { int.TryParse(v, out aMaxSize); SaveSettings(); });
      Seg(d, ref y, S("afps"), new string[] { "60", "30" }, new string[] { "60", "30" }, aMaxFps.ToString(),
        delegate (string v) { int.TryParse(v, out aMaxFps); SaveSettings(); });
      Seg(d, ref y, S("bitrate"), new string[] { S("high"), S("mid"), S("low") }, new string[] { "8", "4", "2" }, aBitRate.ToString(),
        delegate (string v) { int.TryParse(v, out aBitRate); SaveSettings(); });
      Seg(d, ref y, S("ifps"), new string[] { "60", "30" }, new string[] { "60", "30" }, iosFps.ToString(),
        delegate (string v) { int.TryParse(v, out iosFps); SaveSettings(); });
      Seg(d, ref y, S("topmost"), new string[] { S("yes"), S("no") }, new string[] { "1", "0" }, topMost ? "1" : "0",
        delegate (string v) { topMost = (v == "1"); this.TopMost = topMost; SaveSettings(); });
      Seg(d, ref y, S("screenoff"), new string[] { S("yes"), S("no") }, new string[] { "1", "0" }, screenOff ? "1" : "0",
        delegate (string v) { screenOff = (v == "1"); SaveSettings(); });
      Seg(d, ref y, S("micrec"), new string[] { S("yes"), S("no") }, new string[] { "1", "0" }, recMic ? "1" : "0",
        delegate (string v) { recMic = (v == "1"); SaveSettings(); });

      // ── Бағдарлама туралы (құрастырушы) ──
      Panel aboutSep = new Panel(); aboutSep.Size = new Size(404, 1); aboutSep.Location = new Point(22, y); aboutSep.BackColor = Brand.Hair; d.Controls.Add(aboutSep);
      y += 14;
      Label about = new Label();
      about.AutoSize = false; about.Size = new Size(406, 56); about.Location = new Point(24, y);
      about.ForeColor = Brand.FgMute; about.Font = new Font("Segoe UI", 9.5f); about.TextAlign = ContentAlignment.TopLeft;
      about.Text = "ONAIR  ·  " + APP_VERSION + "\nҚұрастырушы / Developer: Жамбыл\n© 2026  ·  ашық бастапқы: UxPlay · scrcpy · GStreamer";
      d.Controls.Add(about);
      y += 60;
      // Instagram — басуға болатын сілтеме
      LinkLabel ig = new LinkLabel();
      ig.Text = "Instagram: @jambyyl"; ig.AutoSize = true; ig.Location = new Point(24, y);
      ig.Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold);
      ig.LinkColor = Brand.Viobl; ig.ActiveLinkColor = Brand.Fg; ig.VisitedLinkColor = Brand.Viobl;
      ig.LinkBehavior = LinkBehavior.HoverUnderline; ig.BackColor = Color.Transparent; ig.Cursor = Cursors.Hand;
      string igUrl = "https://www.instagram.com/jambyyl";
      ig.LinkClicked += delegate { try { Process.Start(igUrl); } catch {} };
      d.Controls.Add(ig);
      y += 34;

      RoundButton close = new RoundButton();
      close.Text = S("close"); close.Font = new Font("Segoe UI Semibold", 10.5f, FontStyle.Bold);
      close.Style = RoundButton.Mode.Gradient; close.Radius = 21; close.Size = new Size(190, 42);
      close.Location = new Point((450 - 190) / 2, y + 8); close.ForeColor = Color.White; close.Cursor = Cursors.Hand;
      close.Click += delegate { d.Close(); }; d.Controls.Add(close);
      d.ShowDialog(this);
    }

    void Seg(Form d, ref int y, string label, string[] names, string[] values, string current, Action<string> onPick) {
      Label l = new Label(); l.Text = label; l.ForeColor = Brand.FgMute; l.Font = new Font("Segoe UI Semibold", 9, FontStyle.Bold);
      l.AutoSize = false; l.Size = new Size(404, 20); l.Location = new Point(24, y); d.Controls.Add(l);
      int x = 22, by = y + 22;
      RoundButton[] pills = new RoundButton[names.Length];
      for (int k = 0; k < names.Length; k++) {
        RoundButton b = new RoundButton();
        b.Text = names[k]; b.Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold);
        Size m = TextRenderer.MeasureText(names[k], b.Font); int w = Math.Max(58, m.Width + 24);
        b.Size = new Size(w, 32); b.Location = new Point(x, by); b.Radius = 16; b.Cursor = Cursors.Hand;
        bool active = values[k] == current;
        b.Style = active ? RoundButton.Mode.Gradient : RoundButton.Mode.Ghost;
        b.BackColor = Brand.Surf; b.Border = Brand.Hair; b.ForeColor = active ? Color.White : Brand.Fg;
        int kk = k;
        b.Click += delegate {
          for (int j = 0; j < pills.Length; j++) {
            pills[j].Style = (j == kk) ? RoundButton.Mode.Gradient : RoundButton.Mode.Ghost;
            pills[j].ForeColor = (j == kk) ? Color.White : Brand.Fg; pills[j].Invalidate();
          }
          onPick(values[kk]);
        };
        d.Controls.Add(b); pills[k] = b; x += w + 8;
      }
      y = by + 44;
    }

    void DrawGear(Graphics g, float cx, float cy, float r, Color c) {
      // Толтырылған тісті дөңгелек (трапеция тістер) + ортасында тесік
      int teeth = 8;
      float rOut = r, rMid = r * 0.72f, rHole = r * 0.42f;
      double step = 2 * Math.PI / teeth;
      List<PointF> pts = new List<PointF>();
      double[] frac = { -0.5, -0.34, -0.28, 0.28, 0.34 };
      float[] rad = { rMid, rMid, rOut, rOut, rMid };
      for (int i = 0; i < teeth; i++) {
        double th = i * step;
        for (int k = 0; k < 5; k++) {
          double a = th + frac[k] * step;
          pts.Add(new PointF((float)(cx + Math.Cos(a) * rad[k]), (float)(cy + Math.Sin(a) * rad[k])));
        }
      }
      using (GraphicsPath path = new GraphicsPath()) {
        path.AddPolygon(pts.ToArray());
        path.AddEllipse(cx - rHole, cy - rHole, rHole * 2, rHole * 2);   // ортадағы тесік
        path.FillMode = FillMode.Alternate;                              // тесік «ойылады»
        using (Brush b = new SolidBrush(c)) g.FillPath(b, path);
      }
    }

    // ── Жолақты салу: логотип + ONAIR + ЭФИРДЕ пилл ──────────
    void BarPaint(object s, PaintEventArgs e) {
      Graphics g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
      using (Pen pn = new Pen(Brand.Hair, 1)) g.DrawLine(pn, 0, bar.Height - 1, bar.Width, bar.Height - 1);
      int lx = 18, lh = 30, cy = (bar.Height - lh) / 2;
      int lw = lh;
      if (logoBmp != null) {
        lw = (int)((float)lh * logoBmp.Width / logoBmp.Height);
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.DrawImage(logoBmp, lx, cy, lw, lh);
      }
      int tx = lx + lw + 6;
      TextRenderer.DrawText(g, "AIR", fWord, new Point(tx, cy + 2), Brand.Fg, TextFormatFlags.NoPadding);
      int airW = TextRenderer.MeasureText(g, "AIR", fWord).Width;
      float gx = tx + airW + 26, gy = bar.Height / 2f;
      gearRect = new Rectangle((int)gx - 15, (int)gy - 15, 30, 30);
      DrawGear(g, gx, gy, 13f, gearHot ? Brand.Fg : Brand.FgMute);
    }

    // ── Видео аймағы: idle кезде ambient glow + нұсқаулық ────
    void VideoPaint(object s, PaintEventArgs e) {
      Graphics g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
      bool idle = !(embIos || embAndroid);
      g.Clear(Brand.Bg1);
      if (!idle) return;
      int W = video.Width, H = video.Height;
      // ambient свечение
      DrawGlow(g, new Rectangle((int)(W * 0.62), -90, 320, 320), Color.FromArgb(60, Brand.Violet));
      DrawGlow(g, new Rectangle(-120, (int)(H * 0.12), 300, 300), Color.FromArgb(42, Brand.Blue));
      // eyebrow
      string eye = "iOS · ANDROID · WIRELESS";
      Size es = TextRenderer.MeasureText(g, eye, fEyebrow);
      int ey = (int)(H * 0.30);
      TextRenderer.DrawText(g, eye, fEyebrow, new Point((W - es.Width) / 2, ey), Brand.Indigo, TextFormatFlags.NoPadding);
      // тақырып
      string h1 = S("idle_t");
      using (Font f = new Font("Segoe UI Semibold", 16, FontStyle.Bold)) {
        Size hs = TextRenderer.MeasureText(g, h1, f);
        TextRenderer.DrawText(g, h1, f, new Point((W - hs.Width) / 2, ey + 26), Brand.Fg, TextFormatFlags.NoPadding);
      }
      string body = S("hint");
      Rectangle br = new Rectangle((W - 440) / 2, ey + 64, 440, 160);
      TextRenderer.DrawText(g, body, fHint, br, Brand.FgMute,
        TextFormatFlags.HorizontalCenter | TextFormatFlags.WordBreak);
    }

    void DrawGlow(Graphics g, Rectangle r, Color c) {
      using (GraphicsPath p = new GraphicsPath()) {
        p.AddEllipse(r);
        using (PathGradientBrush pg = new PathGradientBrush(p)) {
          pg.CenterColor = c;
          pg.SurroundColors = new Color[] { Color.FromArgb(0, c) };
          g.FillEllipse(pg, r);
        }
      }
    }

    bool IosRunning { get { return procIos != null && !procIos.HasExited; } }
    bool AndroidRunning { get { return procAndroid != null && !procAndroid.HasExited; } }

    // ===================== iPhone =====================
    void ToggleIos() { if (IosRunning) StopIos(); else StartIos(); }
    void StartIos() {
      if (!File.Exists(uxplay)) { MessageBox.Show("uxplay.exe табылмады:\n" + uxplay, "ONAIR", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
      KillProcs("uxplay");
      embIos = false; childIos = IntPtr.Zero; natIos = Size.Empty;
      ProcessStartInfo psi = new ProcessStartInfo();
      psi.FileName = uxplay;
      string ia = string.Join(" ", uxArgs) + " -fps " + iosFps;
      psi.Arguments = ia;
      psi.UseShellExecute = false; psi.CreateNoWindow = true;
      psi.WorkingDirectory = Path.Combine(baseDir, "bin");
      try { procIos = Process.Start(psi); }
      catch (Exception ex) { MessageBox.Show("Қате: " + ex.Message, "ONAIR"); return; }
      UpdateUi();
    }
    void StopIos() {
      try { if (procIos != null && !procIos.HasExited) procIos.Kill(); } catch {}
      KillProcs("uxplay");
      procIos = null; childIos = IntPtr.Zero; embIos = false; natIos = Size.Empty;
      UpdateUi(); Relayout();
    }

    // ===================== Android =====================
    void ToggleAndroid() { if (AndroidRunning) StopAndroid(); else StartAndroid(); }
    void StartAndroid() {
      if (!File.Exists(scrcpy)) { MessageBox.Show("scrcpy табылмады:\n" + scrcpy, "ONAIR", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
      string dev = LoadDevice();
      if (!reconnecting && !AndroidRunning && phones.Count > 1) {  // жаңа қосу: телефонды таңдау
        string picked = PickPhone();
        if (picked == null) return;
        dev = picked; SaveDevice(dev);
      } else if (dev.Length == 0 && phones.Count >= 1) {
        dev = phones[0];
      }
      if (!string.IsNullOrEmpty(dev)) RunAdb("connect " + dev);
      if (!AdbDeviceReady()) {
        string input = Microsoft.VisualBasic.Interaction.InputBox(
          S("not_conn"), "ONAIR — Android (Wi-Fi)", dev);
        if (string.IsNullOrEmpty(input)) return;
        dev = input.Trim();
        RunAdb("connect " + dev);
        if (!AdbDeviceReady()) {
          MessageBox.Show(dev + "\r\n\r\n" + S("conn_fail2"), "ONAIR — Android", MessageBoxButtons.OK, MessageBoxIcon.Warning);
          return;
        }
        SaveDevice(dev);
      }
      string serial = GetSerial();
      KillProcs("scrcpy");
      embAndroid = false; childAndroid = IntPtr.Zero; natAndroid = Size.Empty;
      ProcessStartInfo psi = new ProcessStartInfo();
      psi.FileName = scrcpy;
      string aa = (serial.Length > 0 ? "-s " + serial + " " : "") + "--window-title=ONAIR-Android --stay-awake";
      if (aMaxSize > 0) aa += " --max-size=" + aMaxSize;
      aa += " --max-fps=" + aMaxFps + " --video-bit-rate=" + aBitRate + "M";
      if (androidLandscape) aa += " --display-orientation=90";
      if (screenOff) aa += " --turn-screen-off";
      psi.Arguments = aa;
      psi.UseShellExecute = false; psi.CreateNoWindow = true;
      psi.WorkingDirectory = scrcpyDir;
      try { procAndroid = Process.Start(psi); }
      catch (Exception ex) { MessageBox.Show("Қате: " + ex.Message, "ONAIR"); return; }
      androidWantOn = true; reconnTries = 0;
      UpdateUi();
    }
    void StopAndroid() {
      androidWantOn = false; reconnTries = 0;
      try { if (procAndroid != null && !procAndroid.HasExited) procAndroid.Kill(); } catch {}
      KillProcs("scrcpy");
      procAndroid = null; childAndroid = IntPtr.Zero; embAndroid = false; natAndroid = Size.Empty;
      UpdateUi(); Relayout();
    }

    string LoadDevice() { try { if (File.Exists(deviceFile)) return File.ReadAllText(deviceFile).Trim(); } catch {} return ""; }
    void SaveDevice(string d) { try { File.WriteAllText(deviceFile, d); } catch {} }

    string RunAdb(string args) {
      try {
        ProcessStartInfo psi = new ProcessStartInfo(adb, args);
        psi.UseShellExecute = false; psi.CreateNoWindow = true;
        psi.RedirectStandardOutput = true; psi.RedirectStandardError = true;
        psi.WorkingDirectory = scrcpyDir;
        Process p = Process.Start(psi);
        string o = p.StandardOutput.ReadToEnd() + "\n" + p.StandardError.ReadToEnd();
        p.WaitForExit(8000);
        return o;
      } catch (Exception ex) { return "ERR:" + ex.Message; }
    }
    bool AdbDeviceReady() {
      string o = RunAdb("devices");
      foreach (string line in o.Split('\n')) {
        string t = line.Trim();
        if (t.Length == 0 || t.StartsWith("List of")) continue;
        string[] p = t.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        if (p.Length >= 2 && p[p.Length - 1] == "device") return true;
      }
      return false;
    }
    string GetSerial() {
      string o = RunAdb("devices"); string first = "";
      foreach (string line in o.Split('\n')) {
        string t = line.Trim();
        if (t.Length == 0 || t.StartsWith("List of")) continue;
        string[] p = t.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        if (p.Length >= 2 && p[p.Length - 1] == "device") {
          string ser = p[0];
          if (ser.Contains(":") && !ser.Contains("_adb")) return ser;
          if (first.Length == 0) first = ser;
        }
      }
      return first;
    }
    void KillProcs(string name) {
      try { foreach (Process p in Process.GetProcessesByName(name)) { try { p.Kill(); } catch {} } } catch {}
    }

    // ===================== Жазып алу (.mp4) =====================
    void ToggleRecord() {
      if (!IosRunning && !AndroidRunning) return;
      if (!recording) {
        // Жазуды бастау: видео-панельді бөлек GStreamer процесімен .mp4-ке жазамыз.
        // uxplay/scrcpy-ге МҮЛДЕ тиіспейміз → AirPlay/Android ағыны үзілмейді, қайта таңдау қажет емес.
        try { Directory.CreateDirectory(recDir); } catch {}
        string ts = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string who = (IosRunning && AndroidRunning) ? "ONAIR" : (IosRunning ? "iPhone" : "Android");
        recPath = Path.Combine(recDir, who + "_" + ts + ".mp4");
        // Бір ғана телефон болса — оның НАҚ бейне аймағын (бос жолақсыз), екеуі болса — бүкіл панельді
        Rectangle rc;
        if (IosRunning && !AndroidRunning) rc = PanelVideoRect(natIos);
        else if (AndroidRunning && !IosRunning) rc = PanelVideoRect(natAndroid);
        else rc = video.RectangleToScreen(video.ClientRectangle);
        recProc = SpawnCapture(recPath, rc);
        if (recProc == null) { recPath = null; return; }
        recStart = DateTime.Now;
        recording = true;
        UpdateUi();
      } else {
        // Жазуды тоқтату: процесті жабамыз (moov үнемі жаңарып тұрғандықтан файл бүтін)
        recording = false;
        try { if (recProc != null && !recProc.HasExited) recProc.Kill(); } catch {}
        recProc = null;
        KillProcs("gst-launch-1.0");
        UpdateUi();
        try {
          if (recPath != null && File.Exists(recPath)) Process.Start("explorer.exe", "/select,\"" + recPath + "\"");
          else Process.Start("explorer.exe", "\"" + recDir + "\"");
        } catch {}
        recPath = null;
      }
    }
    // Берілген экран тіктөртбұрышын .mp4-ке жазатын GStreamer процесін бастау (crash-safe moov)
    Process SpawnCapture(string outPath, Rectangle rc) {
      string gst = Path.Combine(baseDir, "bin", "gst-launch-1.0.exe");
      if (!File.Exists(gst)) { MessageBox.Show("gst-launch-1.0.exe табылмады:\n" + gst, "ONAIR", MessageBoxButtons.OK, MessageBoxIcon.Error); return null; }
      IntPtr hmon = MonitorFromWindow(this.Handle, 2);              // ONAIR қай мониторда тұр
      Rectangle mb = Screen.FromHandle(this.Handle).Bounds;         // сол монитордың шекарасы (виртуал координат)
      double scale = 1.0;
      try { uint dx, dy; if (GetDpiForMonitor(hmon, 0, out dx, out dy) == 0 && dx > 0) scale = dx / 96.0; } catch {}
      // қию координатасын СОЛ МОНИТОРДЫҢ басынан есептейміз (физикалық пиксель)
      int cx = (int)Math.Round((rc.X - mb.X) * scale), cy = (int)Math.Round((rc.Y - mb.Y) * scale);
      int cw = (int)Math.Round(rc.Width * scale), ch = (int)Math.Round(rc.Height * scale);
      cw -= cw % 2; ch -= ch % 2;
      if (cw < 32 || ch < 32) { MessageBox.Show("Жазу аймағы тым кіші.", "ONAIR"); return null; }
      int br = 10000000;   // 10 Mbps — сапа үшін
      // Дыбыс: телефон үні = жүйелік шығысты (loopback) түсіру. Микрофон қосулы болса — араластырамыз.
      string aud;
      if (recMic)
        aud = " wasapisrc loopback=true low-latency=true ! audioconvert ! audioresample ! queue ! amix."
            + " wasapisrc low-latency=true ! audioconvert ! audioresample ! queue ! amix."
            + " audiomixer name=amix ! avenc_aac bitrate=160000 ! aacparse ! queue ! mux.";
      else
        aud = " wasapisrc loopback=true low-latency=true ! audioconvert ! audioresample ! avenc_aac bitrate=160000 ! aacparse ! queue ! mux.";
      string pipe = "mp4mux name=mux reserved-max-duration=7200000000000 reserved-moov-update-period=1000000000 ! filesink location=" + outPath.Replace('\\', '/')
        + " d3d11screencapturesrc monitor-handle=" + hmon.ToInt64() + " show-cursor=false"
        + " crop-x=" + cx + " crop-y=" + cy + " crop-width=" + cw + " crop-height=" + ch
        + " ! d3d11download ! videoconvert ! videorate ! video/x-raw,framerate=30/1"
        + " ! openh264enc bitrate=" + br + " complexity=high ! h264parse ! queue ! mux."
        + aud;
      ProcessStartInfo psi = new ProcessStartInfo();
      psi.FileName = gst; psi.Arguments = pipe;
      psi.UseShellExecute = false; psi.CreateNoWindow = true;
      psi.WorkingDirectory = Path.Combine(baseDir, "bin");
      try { return Process.Start(psi); }
      catch (Exception ex) { MessageBox.Show("Қате: " + ex.Message, "ONAIR"); return null; }
    }

    // Панельдегі телефон бейнесінің НАҚ аймағы (бос жолақтарсыз; аспект сақталады)
    Rectangle PanelVideoRect(Size nat) {
      Rectangle p = video.RectangleToScreen(video.ClientRectangle);
      if (nat.Width <= 0 || nat.Height <= 0) return p;
      double a = (double)nat.Width / nat.Height, pa = (double)p.Width / p.Height;
      int vw, vh, ox, oy;
      if (pa > a) { vh = p.Height; vw = (int)Math.Round(vh * a); ox = (p.Width - vw) / 2; oy = 0; }
      else { vw = p.Width; vh = (int)Math.Round(vw / a); ox = 0; oy = (p.Height - vh) / 2; }
      return new Rectangle(p.X + ox, p.Y + oy, vw, vh);
    }

    void WaitExit(Process p, int ms) { try { if (p != null && !p.HasExited) p.WaitForExit(ms); } catch {} }

    // ===================== Жаңа телефон жұптау терезесі =====================
    void AddPhone() {
      Form d = new Form();
      d.Text = S("add_title");
      d.ClientSize = new Size(500, 600);
      d.StartPosition = FormStartPosition.CenterParent;
      d.FormBorderStyle = FormBorderStyle.FixedDialog;
      d.MaximizeBox = false; d.MinimizeBox = false;
      d.BackColor = Brand.Bg; d.Font = new Font("Segoe UI", 9);
      try { d.Icon = this.Icon; } catch {}
      d.Shown += delegate { try { int v = light ? 0 : 1; DwmSetWindowAttribute(d.Handle, 20, ref v, 4); } catch {} };

      Label eyebrow = new Label();
      eyebrow.Text = "WIRELESS · ADB"; eyebrow.Font = fEyebrow; eyebrow.ForeColor = Brand.Indigo;
      eyebrow.AutoSize = true; eyebrow.Location = new Point(22, 18); d.Controls.Add(eyebrow);

      Label title = new Label();
      title.Text = S("add_title"); title.Font = new Font("Segoe UI Semibold", 15, FontStyle.Bold);
      title.ForeColor = Brand.Fg; title.AutoSize = true; title.Location = new Point(20, 38); d.Controls.Add(title);

      RoundPanel card = new RoundPanel();
      card.Location = new Point(22, 76); card.Size = new Size(456, 140);
      card.Fill = Brand.Surf; card.Border = Brand.Hair; card.Radius = 16;
      d.Controls.Add(card);
      Label steps = new Label();
      steps.Text = S("add_steps");
      steps.ForeColor = Brand.FgMute; steps.Font = new Font("Segoe UI", 8.75f);
      steps.Dock = DockStyle.Fill; steps.Padding = new Padding(14, 12, 8, 8);
      steps.BackColor = Color.Transparent;
      card.Controls.Add(steps);

      int y = 232;
      TextBox tIp   = AddField(d, S("f_ip"), ref y);
      TextBox tPair = AddField(d, S("f_pair"), ref y);
      TextBox tPin  = AddField(d, S("f_pin"), ref y);
      TextBox tConn = AddField(d, S("f_conn"), ref y);
      try { string sv = LoadDevice(); if (sv.Contains(":")) tIp.Text = sv.Split(':')[0]; } catch {}

      Label st = new Label();
      st.ForeColor = Brand.FgMute; st.Font = new Font("Segoe UI", 9.25f);
      st.AutoSize = false; st.Size = new Size(456, 34); st.Location = new Point(22, y + 4); d.Controls.Add(st);

      RoundButton bPair = new RoundButton();
      bPair.Text = S("pair_save"); bPair.Font = new Font("Segoe UI Semibold", 10.5f, FontStyle.Bold);
      bPair.Style = RoundButton.Mode.Gradient; bPair.Radius = 21;
      bPair.Size = new Size(250, 44); bPair.Location = new Point(22, y + 42);
      bPair.ForeColor = Color.White; bPair.Cursor = Cursors.Hand; d.Controls.Add(bPair);

      RoundButton bCancel = new RoundButton();
      bCancel.Text = S("cancel"); bCancel.Font = new Font("Segoe UI", 10.5f);
      bCancel.Style = RoundButton.Mode.Ghost; bCancel.Radius = 21; bCancel.Border = Brand.Hair;
      bCancel.Size = new Size(150, 44); bCancel.Location = new Point(284, y + 42);
      bCancel.BackColor = Brand.Surf; bCancel.ForeColor = Brand.Fg; bCancel.Cursor = Cursors.Hand;
      bCancel.Click += delegate { d.Close(); }; d.Controls.Add(bCancel);

      bPair.Click += delegate {
        string ip = tIp.Text.Trim(), pp = tPair.Text.Trim(), pin = tPin.Text.Trim(), cp = tConn.Text.Trim();
        if (ip.Length == 0 || pp.Length == 0 || pin.Length == 0 || cp.Length == 0) {
          st.ForeColor = Brand.Live; st.Text = S("fill_all"); return;
        }
        bPair.Enabled = false; st.ForeColor = Brand.FgMute; st.Text = S("pairing"); d.Refresh(); Application.DoEvents();
        string r1 = RunAdb("pair " + ip + ":" + pp + " " + pin);
        if (r1.IndexOf("Successfully paired", StringComparison.OrdinalIgnoreCase) < 0) {
          st.ForeColor = Brand.Live; st.Text = S("pair_fail");
          bPair.Enabled = true; return;
        }
        st.ForeColor = Brand.FgMute; st.Text = S("paired_conn"); d.Refresh(); Application.DoEvents();
        RunAdb("connect " + ip + ":" + cp);
        if (!AdbDeviceReady()) {
          st.ForeColor = Brand.Live; st.Text = S("conn_fail");
          bPair.Enabled = true; return;
        }
        AddPhoneToList(ip + ":" + cp);
        st.ForeColor = Brand.Mint; st.Text = S("saved_ok");
        MessageBox.Show(S("save_msg"), "ONAIR", MessageBoxButtons.OK, MessageBoxIcon.Information);
        d.Close();
      };

      d.ShowDialog(this);
    }

    TextBox AddField(Form d, string label, ref int y) {
      Label l = new Label();
      l.Text = label; l.ForeColor = Brand.FgMute; l.Font = new Font("Segoe UI Semibold", 9, FontStyle.Bold);
      l.AutoSize = false; l.Size = new Size(150, 22); l.Location = new Point(24, y); d.Controls.Add(l);

      RoundPanel fp = new RoundPanel();
      fp.Location = new Point(22, y + 22); fp.Size = new Size(456, 36);
      fp.Fill = Brand.Bg1; fp.Border = Brand.Hair2; fp.Radius = 11; d.Controls.Add(fp);

      TextBox t = new TextBox();
      t.BorderStyle = BorderStyle.None; t.Font = fMono;
      t.BackColor = Brand.Bg1; t.ForeColor = Brand.Fg;
      t.Location = new Point(12, 8); t.Size = new Size(432, 22);
      fp.Controls.Add(t);
      t.Enter += delegate { fp.Border = Brand.Viobl; fp.Invalidate(); };
      t.Leave += delegate { fp.Border = Brand.Hair2; fp.Invalidate(); };
      y += 64;
      return t;
    }

    // ===================== Таймер / ендіру / орналасу =====================
    void OnTick(object s, EventArgs e) {
      if (IosRunning && !embIos) TryEmbed(true);
      if (AndroidRunning && !embAndroid) TryEmbed(false);
      if (procIos != null && procIos.HasExited) { procIos = null; childIos = IntPtr.Zero; embIos = false; UpdateUi(); Relayout(); }
      if (procAndroid != null && procAndroid.HasExited) { procAndroid = null; childAndroid = IntPtr.Zero; embAndroid = false; UpdateUi(); Relayout(); }
      // Жазу таймері
      if (recording) { btnRec.Text = "■  " + RecElapsed(); btnRec.Invalidate(); }
      // Авто-қайта қосылу (Android үзілсе)
      if (androidWantOn && !AndroidRunning && (DateTime.Now - lastReconnectAt).TotalSeconds >= 2.5) {
        lastReconnectAt = DateTime.Now; reconnTries++;
        if (reconnTries > 20) { androidWantOn = false; UpdateUi(); Relayout(); }
        else {
          string dev = LoadDevice();
          if (dev.Length > 0) RunAdb("connect " + dev);
          if (AdbDeviceReady()) { reconnTries = 0; reconnecting = true; StartAndroid(); reconnecting = false; }
        }
      }
      if (annotating && !embIos && !embAndroid) ToggleAnnot();
    }
    string RecElapsed() {
      TimeSpan t = DateTime.Now - recStart;
      return ((int)t.TotalMinutes).ToString("00") + ":" + t.Seconds.ToString("00");
    }

    void TryEmbed(bool ios) {
      Process p = ios ? procIos : procAndroid;
      if (p == null || p.HasExited) return;
      IntPtr h = FindProcWindow((uint)p.Id);
      if (h == IntPtr.Zero) return;
      Size nat = new Size(440, 760);
      try { RECT r; GetWindowRect(h, out r); int vw = r.Right - r.Left, vh = r.Bottom - r.Top; if (vw > 0 && vh > 0) nat = new Size(vw, vh); } catch {}
      AttachChild(h);
      if (ios) { childIos = h; natIos = nat; embIos = true; }
      else     { childAndroid = h; natAndroid = nat; embAndroid = true; }
      Relayout();
    }
    void AttachChild(IntPtr c) {
      uint style = (uint)GetWindowLong(c, GWL_STYLE);
      style &= ~(WS_CAPTION | WS_THICKFRAME | WS_POPUP | WS_MINIMIZEBOX | WS_MAXIMIZEBOX | WS_SYSMENU);
      style |= WS_CHILD | WS_VISIBLE;
      SetWindowLong(c, GWL_STYLE, (int)style);
      SetParent(c, video.Handle);
    }
    IntPtr FindProcWindow(uint pid) {
      IntPtr found = IntPtr.Zero;
      EnumWindows(delegate (IntPtr h, IntPtr l) {
        uint wp; GetWindowThreadProcessId(h, out wp);
        if (wp == pid && IsWindowVisible(h) && GetWindowTextLength(h) > 0) { found = h; return false; }
        return true;
      }, IntPtr.Zero);
      return found;
    }

    void Relayout() {
      bool i = embIos && childIos != IntPtr.Zero;
      bool a = embAndroid && childAndroid != IntPtr.Zero;
      if (!i && !a) { video.Invalidate(); LayoutChildren(); return; }
      Rectangle wa = Screen.FromControl(this).WorkingArea;
      int maxH = (int)(wa.Height * 0.86);
      int H = Math.Min(820, maxH);
      double arI = (natIos.Height > 0) ? natIos.Width / (double)natIos.Height : 0.46;
      double arA = (natAndroid.Height > 0) ? natAndroid.Width / (double)natAndroid.Height : 0.46;
      int totalW;
      if (i && a) totalW = (int)(H * arI) + (int)(H * arA);
      else if (i) totalW = (int)(H * arI);
      else        totalW = (int)(H * arA);
      int maxW = (int)(wa.Width * 0.95);
      if (totalW > maxW) { double sc = maxW / (double)totalW; H = (int)(H * sc); totalW = (int)(totalW * sc); }
      if (totalW < MinimumSize.Width) totalW = MinimumSize.Width;
      int bot = (tools != null && tools.Visible) ? tools.Height : 0;
      ClientSize = new Size(totalW, bar.Height + bot + H);
      Left = wa.X + (wa.Width - Width) / 2;
      Top  = wa.Y + (wa.Height - Height) / 2;
      LayoutChildren();
    }

    void LayoutChildren() {
      bool i = embIos && childIos != IntPtr.Zero;
      bool a = embAndroid && childAndroid != IntPtr.Zero;
      int w = video.Width, h = video.Height;
      if (w <= 0 || h <= 0) return;
      if (i && a) {
        double arI = (natIos.Height > 0) ? natIos.Width / (double)natIos.Height : 0.46;
        double arA = (natAndroid.Height > 0) ? natAndroid.Width / (double)natAndroid.Height : 0.46;
        int wi = (int)(h * arI), wAn = (int)(h * arA); int sum = wi + wAn; if (sum <= 0) sum = 1;
        wi = (int)((long)w * wi / sum);
        MoveWindow(childIos, 0, 0, wi, h, true);
        MoveWindow(childAndroid, wi, 0, w - wi, h, true);
      } else if (i) { MoveWindow(childIos, 0, 0, w, h, true); }
      else if (a) { MoveWindow(childAndroid, 0, 0, w, h, true); }
    }

    void UpdateUi() {
      SetPill(btnIos, IosRunning, "iPhone");
      SetPill(btnAndroid, AndroidRunning, "Android");
      bool live = IosRunning || AndroidRunning;
      if (!fullscreen) tools.Visible = live;   // төменгі басқару жолағы эфирде
      LayoutTools();
      if (recording) {
        btnRec.Style = RoundButton.Mode.Solid; btnRec.BackColor = Brand.Live; btnRec.ForeColor = Color.White;
        btnRec.Shape = RoundButton.Ico.None; btnRec.Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold);
        btnRec.Width = 104; btnRec.Text = "■  " + RecElapsed();
      } else {
        btnRec.Style = RoundButton.Mode.Ghost; btnRec.BackColor = Brand.Surf; btnRec.ForeColor = Brand.Live;
        btnRec.Shape = RoundButton.Ico.Dot; btnRec.Width = 36; btnRec.Text = "";
      }
      btnRec.Invalidate(); LayoutTools();
      if (live)
        Text = (IosRunning && AndroidRunning) ? "ONAIR — iPhone + Android" : (IosRunning ? "ONAIR — iPhone" : "ONAIR — Android");
      else Text = "ONAIR";
      bar.Invalidate(); video.Invalidate();
    }
    void SetPill(RoundButton b, bool live, string name) {
      if (live) { b.Style = RoundButton.Mode.Solid; b.BackColor = Brand.Live; b.ForeColor = Color.White; b.Text = "■  " + name; }
      else      { b.Style = RoundButton.Mode.Ghost; b.BackColor = Brand.Surf; b.ForeColor = Brand.Fg; b.Text = "▶  " + name; }
      b.Invalidate();
    }
  }
}
