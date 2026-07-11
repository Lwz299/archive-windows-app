using Microsoft.Web.WebView2.WinForms;

namespace Archive.Desktop;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    private WebView2 webView;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        webView = new WebView2();
        SuspendLayout();

        webView.CreationProperties = null;
        webView.DefaultBackgroundColor = System.Drawing.Color.White;
        webView.Dock = System.Windows.Forms.DockStyle.Fill;
        webView.Location = new System.Drawing.Point(0, 0);
        webView.Name = "webView";
        webView.Size = new System.Drawing.Size(800, 450);
        webView.TabIndex = 0;

        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(800, 450);
        Controls.Add(webView);
        Name = "Form1";
        Text = "أرشيف الكتب";
        ResumeLayout(false);
    }

    #endregion
}
