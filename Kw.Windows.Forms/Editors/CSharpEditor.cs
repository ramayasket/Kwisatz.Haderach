using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Kw.Windows.Forms.FastColored;

namespace Kw.Windows.Forms.Editors
{
    public class CSharpEditor : FastColoredTextBox
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
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

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new Container();
            AutoScaleMode = AutoScaleMode.Font;
        }

        public CSharpEditor(string font, float size) : this()
        {
            Font = new Font(font, size);
        }

        public CSharpEditor()
        {
            InitializeComponent();
            
            Dock = DockStyle.Fill;
            BorderStyle = BorderStyle.Fixed3D;
            VirtualSpace = true;
            
            LeftPadding = 17;
            Language = Language.CSharp;
            Focus();
            DelayedTextChangedInterval = 1000;
            DelayedEventsInterval = 500;
            HighlightingRangeType = HighlightingRangeType.VisibleRange;
            SelectionHighlightingForLineBreaksEnabled = true;
        }
    }
}
