using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SimpleTextEditor
{
    class TextEditor : Form
    {
        private MenuStrip menu;
        private Panel basicPanel;
        private RichTextBox textArea;
        private ContextMenuStrip contextMenuStrip;
        private ToolStrip searchBar;
        private ToolStripButton caseSensitiveButton;
        private ToolStripButton wholeWordButton;
        private ToolStripButton findButton;
        private ToolStripButton findPreviousButton;
        private ToolStripButton cancelSearchButton;
        private ToolStripTextBox searchTextBox;
        private ToolStrip replaceBar;
        private ToolStripLabel replaceOldTextLabel;
        private ToolStripLabel replaceNewTextLabel;
        private ToolStripButton replaceButton;
        private ToolStripButton cancelReplacementButton;
        private ToolStripTextBox replaceTextBoxOld;
        private ToolStripTextBox replaceTextBoxNew;

        string fileName;
        bool textChanged = false;

        private int searchStartPos;
        private int reverseSearchStartPos;

        public TextEditor(string title)
        {
            this.Text = title;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(800, 600);
            this.AutoScroll = true;

            InitBasicPanel();
            InitTextArea();
            InitContextMenuStrip();
            InitMenu();
            InitSearchBar();
            InitReplacementBar();
        }

        private ToolStripMenuItem CreateMenuItem(string text, Color backcolor, EventHandler eventHandler)
        {
            ToolStripMenuItem menuItem = new ToolStripMenuItem();
            menuItem.Text = text;
            menuItem.BackColor = backcolor;
            menuItem.Click += eventHandler;
            return menuItem;
        }

        private MenuStrip createMenu(string[][] menuItemsNames, EventHandler[][] menuItemsArguments) 
        {
            MenuStrip menu = new MenuStrip();

            for(int i = 0; i < menuItemsNames.GetLength(0); i++)
            {
                ToolStripMenuItem menuItem = CreateMenuItem(menuItemsNames[i][0], Color.Transparent, null);

                for(int j = 1; j < menuItemsNames[i].Length; j++)
                {
                    ToolStripItem subMenuItem = null;

                    if (String.IsNullOrEmpty(menuItemsNames[i][j]))
                    {
                        subMenuItem = new ToolStripSeparator();
                    }
                    else
                    {
                        subMenuItem = CreateMenuItem(menuItemsNames[i][j], Color.GhostWhite, menuItemsArguments[i][j]);
                    }
                    menuItem.DropDownItems.Add(subMenuItem);
                }
                menu.Items.Add(menuItem);
            }
            return menu;
        }

        private void InitMenu()
        {
            string[][] names =
            {
                new string [] {"File", "New", "Open", "Save", "Save As", null, "Exit" },
                new string [] {"Edit", "Redo", "Undo", null, "Copy", "Cut",  "Paste" },
                new string [] {"Search", "Find", null, "Replace"},
                new string [] {"Preferences" , "Background", "Color"},
                new string [] {"Help", "About"}
            };

            EventHandler[][] arguments =
            {
                new EventHandler [] { null, OnClickNew, OnClickOpen, OnClickSave, OnClickSave, null, OnClickExit },
                new EventHandler [] { null, OnClickRedo, OnClickUndo, null, OnCLickCopy, OnClickCut, OnClickPaste },
                new EventHandler [] { null, OnClickFind, null, OnClickReplace },
                new EventHandler [] { null, OnClickBackground, OnClickFont },
                new EventHandler [] { null, OnClickAbout }
            };

            menu = createMenu(names, arguments);
            menu.BackColor = Color.GhostWhite;
            this.Controls.Add(menu);
        }

        private void InitBasicPanel()
        {
            basicPanel = new Panel();
            basicPanel.AutoScroll = true;
            basicPanel.Dock = DockStyle.Fill;

            this.Controls.Add(basicPanel);
        }

        private void InitTextArea()
        {
            textArea = new RichTextBox();
            textArea.Dock = DockStyle.Fill;
            textArea.BorderStyle = BorderStyle.None;
            textArea.Font = new Font("Arial", 14.0f, FontStyle.Regular);
            textArea.TextChanged += HandleTextChanged;
            textArea.KeyDown += HandleHotKeys;
            textArea.ContextMenuStrip = contextMenuStrip;
            textArea.MouseDown += HandleMouseDown;

            basicPanel.Controls.Add(textArea);
        }

        private void InitContextMenuStrip()
        {
            contextMenuStrip = new ContextMenuStrip();

            ToolStripMenuItem copyItem = CreateMenuItem("Copy", Color.GhostWhite, OnCLickCopy);
            ToolStripMenuItem cutItem = CreateMenuItem("Cut", Color.GhostWhite, OnClickCut);
            ToolStripMenuItem pasteItem = CreateMenuItem("Paste", Color.GhostWhite, OnClickPaste);
            
            contextMenuStrip.Items.Add(copyItem);
            contextMenuStrip.Items.Add(cutItem);
            contextMenuStrip.Items.Add(pasteItem);
        }

        private ToolStripButton CreateToolStripButton(string text, string toolTiptext, Padding padding, bool checkOnClick, ToolStripItemAlignment alignment, EventHandler eventHandler)
        {
            ToolStripButton button = new ToolStripButton();
            button.Text = text;
            button.AutoToolTip = false;
            button.ToolTipText = toolTiptext;
            button.Margin = padding;
            button.CheckOnClick = checkOnClick;
            button.Alignment = alignment;
            button.Click += eventHandler;
           
            return button;
        }

        private ToolStripTextBox CreateToolStripTextBox(Size size, Padding padding, String toolTipText, KeyEventHandler keyEventHandler)
        {
            ToolStripTextBox textBox = new ToolStripTextBox();
            textBox.Size = size;
            textBox.Margin = padding;
            textBox.AutoToolTip = false;
            textBox.ToolTipText = toolTipText;

            return textBox;
        }

        private ToolStripLabel CreateToolStripLabel(string text, Padding padding)
        {
            ToolStripLabel label = new ToolStripLabel();
            label.Text = text;
            label.Padding = padding;
            return label;
        }

        private void InitSearchBar()
        {
            searchBar = new ToolStrip();
            searchBar.Dock = DockStyle.Bottom;
            searchBar.Visible = false;
            searchBar.GripStyle = ToolStripGripStyle.Hidden;
            searchBar.BackColor = Color.LightGray;
            searchBar.KeyDown += HandleHotKeys;

            caseSensitiveButton = CreateToolStripButton("Aa", "Case sensitive", new Padding(0,0,0,0), true, ToolStripItemAlignment.Left, null);
            wholeWordButton = CreateToolStripButton(@"""word""", "Whole word", new Padding(4,0,0,0), true, ToolStripItemAlignment.Left, null);
            findButton = CreateToolStripButton("Find", "Find", new Padding(50, 0, 0, 0), false, ToolStripItemAlignment.Right, FindTheNextWord);
            findPreviousButton = CreateToolStripButton("Find Previous", "Find Previous", new Padding(4, 0, 0, 0), false, ToolStripItemAlignment.Right, FindThePreviousWord);
            cancelSearchButton = CreateToolStripButton("Cancel", "Cancel", new Padding(4, 0, 0, 0), false, ToolStripItemAlignment.Right, CancelTheSearch);
            searchTextBox = CreateToolStripTextBox(new Size(400, -1), new Padding(50, 0, 0, 0), "Target word", HandleHotKeys);
            
            searchBar.Items.Add(caseSensitiveButton);
            searchBar.Items.Add(wholeWordButton);
            searchBar.Items.Add(searchTextBox);
            searchBar.Items.Add(cancelSearchButton);
            searchBar.Items.Add(findPreviousButton);
            searchBar.Items.Add(findButton);

            this.Controls.Add(searchBar);
        }
        
        private void InitReplacementBar()
        {
            replaceBar = new ToolStrip();
            replaceBar.Dock = DockStyle.Bottom;
            replaceBar.Visible = false;
            replaceBar.GripStyle = ToolStripGripStyle.Hidden;
            replaceBar.BackColor = Color.LightGray;

            replaceOldTextLabel = CreateToolStripLabel("Replace:", new Padding(5,0,0,0));
            replaceNewTextLabel = CreateToolStripLabel("with:", new Padding(20,0,0,0));
            replaceTextBoxOld = CreateToolStripTextBox(new Size(200,-1),new Padding(5, 0, 0, 0), "Word to be replaced", HandleHotKeys);
            replaceTextBoxNew = CreateToolStripTextBox(new Size(200, -1), new Padding(5, 0, 0, 0), "Word to replace", HandleHotKeys);
            replaceButton = CreateToolStripButton("Replace", "Replace", new Padding(30, 0, 0, 0), false, ToolStripItemAlignment.Right, ReplaceTheWord);
            cancelReplacementButton = CreateToolStripButton("Cancel", "Cancel", new Padding(4, 0, 0, 0), false, ToolStripItemAlignment.Right, CancelTheReplacement);

            replaceBar.Items.Add(replaceOldTextLabel);
            replaceBar.Items.Add(replaceTextBoxOld);
            replaceBar.Items.Add(replaceNewTextLabel);
            replaceBar.Items.Add(replaceTextBoxNew);
            replaceBar.Items.Add(cancelReplacementButton);
            replaceBar.Items.Add(replaceButton);

            this.Controls.Add(replaceBar);
        }

        private void OpenNewWindow()
        {
            Application.Run(new TextEditor("SharpText"));
        }

        private void OnClickNew(object sender, EventArgs e)
        {
            System.Threading.Thread t = new System.Threading.Thread(new System.Threading.ThreadStart(OpenNewWindow));
            t.Start();
        }

        private void OnClickOpen(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.ShowDialog();
            fileName = openFileDialog.FileName;

            try
            {
                textArea.LoadFile(openFileDialog.FileName, RichTextBoxStreamType.PlainText);
                textChanged = false;
            }
            catch(ArgumentException argException)
            {
            }
        }   

        private void OnClickSave(object sender, EventArgs e)
        {
            DialogResult saveAnswer = DialogResult.Cancel;

            if(sender.ToString().Equals("Save As") || String.IsNullOrEmpty(fileName))
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Text Files (*.txt) | *.txt";
                saveAnswer = saveFileDialog.ShowDialog();
                
                if (!String.IsNullOrEmpty(saveFileDialog.FileName))
                {
                    fileName = saveFileDialog.FileName;
                }
            }
            try
            {
                if (saveAnswer == DialogResult.OK)
                {
                    textArea.SaveFile(fileName, RichTextBoxStreamType.PlainText);
                    textChanged = false;
                }
            }
            catch (ArgumentException argException)
            {
            } 
        }

        private void OnClickExit(object sender, EventArgs e)
        {
            if (textChanged)
            {
                string title = "Exit";
                string message = "There are unsaved actions. Do you want to exit?";
                DialogResult exitAnswer =  MessageBox.Show(message, title, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                
                if(exitAnswer != DialogResult.Yes)
                {
                    return;
                }
            }
            this.Close();
        }

        private void OnClickUndo(object sender, EventArgs e)
        {
            textArea.Undo();
        }

        private void OnClickRedo(object sender, EventArgs e)
        {
            textArea.Redo();
        }

        private void OnCLickCopy(object sender, EventArgs e)
        {
            textArea.Copy();
        }

        private void OnClickCut(object sender, EventArgs e)
        {
            textArea.Cut();
        }

        private void OnClickPaste(object sender, EventArgs e)
        {
            textArea.Paste();
        }

        private void OnClickFind(object sender, EventArgs e)
        {
            searchBar.Visible = !searchBar.Visible;
            if (searchBar.Visible)
            {
                this.ActiveControl = searchTextBox.Control;
                replaceBar.Visible = false;
            }
        }

        private void FindTheNextWord(object sender, EventArgs e)
        {
            try
            {
                searchStartPos = FindWord(searchTextBox.Text, searchStartPos, textArea.Text.Length, caseSensitiveButton.Checked, wholeWordButton.Checked, false);
                reverseSearchStartPos = searchStartPos;
                searchStartPos += searchTextBox.Text.Length;
            }
            catch
            {
                searchStartPos = 0;
                searchStartPos = FindWord(searchTextBox.Text, searchStartPos, textArea.Text.Length, caseSensitiveButton.Checked, wholeWordButton.Checked, false);
                reverseSearchStartPos = searchStartPos;
                searchStartPos += searchTextBox.Text.Length;
            }
        }
        
        private void FindThePreviousWord(object sender, EventArgs e)
        {
            try
            {
                reverseSearchStartPos = FindWord(searchTextBox.Text, 0, reverseSearchStartPos, caseSensitiveButton.Checked, wholeWordButton.Checked, true);
                searchStartPos = reverseSearchStartPos + searchTextBox.Text.Length + 1;
            }
            catch
            {
                reverseSearchStartPos = textArea.Text.Length;
                reverseSearchStartPos = FindWord(searchTextBox.Text, 0, reverseSearchStartPos, caseSensitiveButton.Checked, wholeWordButton.Checked, true);
                searchStartPos = reverseSearchStartPos + searchTextBox.Text.Length + 1;
            }
        }
        
        private int FindWord(string word, int startPos, int endPos, bool caseSensitive, bool wholeWord, bool reverseSearch)
        {
            if (reverseSearch)
            {
                if (caseSensitive && wholeWord)
                {
                    return textArea.Find(word, startPos, endPos, RichTextBoxFinds.MatchCase | RichTextBoxFinds.WholeWord | RichTextBoxFinds.Reverse);
                }
                else if (caseSensitive && !wholeWord)
                {
                    return textArea.Find(word, startPos, endPos, RichTextBoxFinds.MatchCase | RichTextBoxFinds.Reverse);
                }
                else if (!caseSensitive && wholeWord)
                {
                    return textArea.Find(word, startPos, endPos, RichTextBoxFinds.WholeWord | RichTextBoxFinds.Reverse);
                }
                else
                {
                    return textArea.Find(word, startPos, endPos, RichTextBoxFinds.Reverse);
                }
            }
            else
            {
                if (caseSensitive && wholeWord)
                {
                    return textArea.Find(word, startPos, endPos, RichTextBoxFinds.MatchCase | RichTextBoxFinds.WholeWord);
                }
                else if (caseSensitive && !wholeWord)
                {
                    return textArea.Find(word, startPos, endPos, RichTextBoxFinds.MatchCase);
                }
                else if (!caseSensitive && wholeWord)
                {
                    return textArea.Find(word, startPos, endPos, RichTextBoxFinds.WholeWord);
                }
                else
                {
                    return textArea.Find(word, startPos, endPos, RichTextBoxFinds.None);
                }
            }
        }

        private void CancelTheSearch(object sender, EventArgs e)
        {
            searchBar.Visible = false;
        }

        private void OnClickReplace(object sender, EventArgs e)
        {
            replaceBar.Visible = !replaceBar.Visible;
            if (replaceBar.Visible)
            {
                this.ActiveControl = replaceTextBoxOld.Control;
                searchBar.Visible = false;
            }
        }

        private void ReplaceTheWord(object sender, EventArgs e)
        {
            textArea.Text = textArea.Text.Replace(replaceTextBoxOld.Text, replaceTextBoxNew.Text);
        }

        private void CancelTheReplacement(object sender, EventArgs e)
        {
            replaceBar.Visible = false;
        }

        private void OnClickBackground(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            DialogResult result = colorDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                textArea.BackColor = colorDialog.Color;
            }
        }

        private void OnClickFont(object sender, EventArgs e)
        {
            FontDialog fontDialog = new FontDialog();
            DialogResult result = fontDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                textArea.Font = fontDialog.Font;
            }

            ColorDialog colorDialog = new ColorDialog();
            result = colorDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                textArea.ForeColor = colorDialog.Color;
            }
        }

        private void OnClickAbout(object sender, EventArgs e)
        {
            string dialogTitle = "Sharp Text Editor - Help";
            string dialogMessage = "This is a simple \"handmade\" text editor. ";
            MessageBoxButtons dialogButtons = MessageBoxButtons.OK;

            MessageBox.Show(dialogMessage, dialogTitle, dialogButtons, MessageBoxIcon.Information);

        }

        private void HandleTextChanged(object sender, EventArgs e)
        {
            textChanged = true;
        }

        private void HandleHotKeys(object sender, KeyEventArgs e)
        {
            if(e.Control && e.KeyCode == Keys.S)
            {
                OnClickSave("Save",null);
            }
            else if (e.Control && e.KeyCode == Keys.F)
            {
                OnClickFind("HandleHotKeys", null);
            }
            else if (e.Control && e.KeyCode == Keys.H)
            {
                OnClickReplace("HandleHotKeys", null);
            }
        }

        private void HandleMouseDown(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Right)
            {
                contextMenuStrip.Show(MousePosition);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                //Escape key press, handle it your way but be sure to return true
                searchBar.Visible = false;
                replaceBar.Visible = false;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
