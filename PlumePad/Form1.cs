using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;

namespace PlumePad
{
    public partial class Form1 : Form
    {
        private bool dirtyFlag = false;     // ファイル変更フラグ
        private bool readOnly = false;      // 読み取り専用フラグ
        private string editFilePath = "";   // 編集ファイルパス
        findDialog findDlg = null;          // 検索ダイアログのインスタンスを格納
        jumpDialog jumpDlg = null;          // ジャンプダイアログボックスのインスタンスを格納

        // SendMessageの呼び出し準備
        [DllImport("User32.Dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        private const int EM_LINEINDEX = 0xBB;
        private const int EM_LINEFROMCHAR = 0xC9;

        public Form1()
        {
            InitializeComponent();

            // Appliction.Idleを登録する
            Application.Idle += new EventHandler(Application_Idle);
        }

        public Form1(string arg)
        {
            InitializeComponent();

            editFilePath = arg;

            // ファイルが存在するか確認
            if(File.Exists(editFilePath)){
                // テキスト形式としてアプリケーションに登録されているかチェック
                if(CheckFileType(editFilePath)){
                    //テキストファイルの内容をテキストボックスにロード
                    textMemo.Text = File.ReadAllText(editFilePath, Encoding.Default);
                    textMemo.SelectionStart = 0;

                    // フォームのタイトル部分にファイル名を表示
                    this.Text = "PlumePad - " + GetFileNameString(arg, '\\');
                }
                else {
                    //　ファイルタイプが登録されていない場合はエラー表示
                    MessageBox.Show("このファイルは開くことが出来ません。\n" + editFilePath, "PlumePad", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }

                setDirty(false);
            }
            else{
                // ファイルが存在しない場合にエラーメッセージを表示
                MessageBox.Show("ファイルが見つかりません。\n" + editFilePath, "PlumePad", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            // Appliction.Idleを登録する
            Application.Idle += new EventHandler(Application_Idle);

        }

        private void setDirty(bool flag)
        {
            dirtyFlag = flag;
            menuSave.Enabled = (readOnly || editFilePath == "") ? false : flag;
            toolSave.Enabled = (readOnly || editFilePath == "") ? false : flag;
        }

        // 保存していない編集中のテキストがある場合に確認するための関数
        private bool confirmDestructionText(string msgboxTitle)
        {
            const string MSG_BOX_STRING = "ファイルは保存されていません。\n編集中のテキストは破棄されます。\nよろしいですか?";

            if(!dirtyFlag){
                return true;
            }

            return(MessageBox.Show(MSG_BOX_STRING, msgboxTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes);
        }

        //フルパスからファイル名のみを取り出す関数
        private string GetFileNameString(string filePath, char separateChar)
        {
            try{
                string[] strArray = filePath.Split(separateChar);
                return strArray[strArray.Length - 1];
            }
            catch{
                return "";
            }
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            int row = 1;
            int col = 1;
            int lineIndex = 0;

            if (textMemo.SelectionLength >= 1) {
                toolCut.Enabled = true;
                toolCopy.Enabled = true;
            }
            else {
                toolCut.Enabled = false;
                toolCopy.Enabled = false;
            }

            if (Clipboard.GetDataObject().GetDataPresent(DataFormats.Text)) {
                toolPaste.Enabled = true;
            }
            else {
                toolPaste.Enabled = false;
            }

            // 現在の行を取得する
            row = SendMessage(textMemo.Handle, EM_LINEFROMCHAR, -1, 0) + 1;

            // 現在の列を取得する
            lineIndex = SendMessage(textMemo.Handle, EM_LINEINDEX, -1, 0);
            col = textMemo.SelectionStart - lineIndex + 1;
            
            toolStripStatusLabel2.Text = row + "行 : " + col + "列";

        }

        private void menuOpenFile_Click(object sender, EventArgs e)
        {
            // ファイルダイアログを開く
            openFileDialog1.ShowDialog();
        }

        private void menuSaveAs_Click(object sender, EventArgs e)
        {
            // ファイルが新規作成だった場合の名前
            const string NEW_FILE_NAME = "新規テキストファイル.txt";

            // 編集中のファイルのフルパスからファイル名だけを取得
            string fileNameString = GetFileNameString(editFilePath, '\\');

            // ファイル名が空白であった場合は　"新規テキストファイル.txt" をセット
            saveFileDialog1.FileName = (fileNameString == "") ? NEW_FILE_NAME : fileNameString;

            // ファイルダイアログを開く
            saveFileDialog1.ShowDialog(this);
        }

        private void menuSave_Click(object sender, EventArgs e)
        {
            const string MSGBOX_TITLE = "上書き保存";

            // 編集中のファイルが存在するとき
            if(File.Exists(editFilePath)){
                try{
                    // テキストボックスの内容をファイルに書き込み
                    File.WriteAllText(editFilePath, textMemo.Text, Encoding.Default);

                    // ダーティーフラグのリセット
                    setDirty(false);
                }
                catch(Exception ex){
                    // ファイルの書き込みでエラーが発生した場合に Exception の内容を表示
                    MessageBox.Show(this, ex.Message, MSGBOX_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else{
                string MSG_BOX_STRING = "ファイル\"" + editFilePath + "\" のパスは正しくありません。\n\nディレクトリが存在するか確認してください。";
                MessageBox.Show(MSG_BOX_STRING, MSGBOX_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void menuNewFile_Click(object sender, EventArgs e)
        {
            const string MSG_BOX_TITLE = "新規作成";

            // 編集中のテキストがないか確認
            if(confirmDestructionText(MSG_BOX_TITLE)){
                // テキストエディタの初期化
                this.Text = "PlumePad - 無題";
                textMemo.Clear();
                editFilePath = "";

                // ダーティーフラグのリセット
                setDirty(false);
            }
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            const string TITLE_EXTN_ReadOnly = " (読み取り専用)";
            const string MSGBOX_TITLE = "ファイルを開く";

            // 選択されたファイルのパスを取得
            editFilePath = openFileDialog1.FileName;

            // ファイルダイアログで読み取り専用が選択されたかどうかの値を取得
            readOnly = openFileDialog1.ReadOnlyChecked;

            // 読み取り専用で開いた場合にタイトル(ファイル名)に "読み取り専用" の文字をつける
            this.Text = (readOnly) ? "PlumePad - " + openFileDialog1.SafeFileName + TITLE_EXTN_ReadOnly : "PlumePad - " + openFileDialog1.SafeFileName;

            // テキストボックスにテキストファイルを読み込む
            try{
                //テキストファイルの内容をテキストボックスにロード
                textMemo.Text = File.ReadAllText(editFilePath, Encoding.Default);
            }
            catch(Exception ex){
                //ファイルの読み込みでエラーが発生した場合に Exception の内容を表示
                MessageBox.Show(this, ex.Message, MSGBOX_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // ダーティーフラグのリセット
            setDirty(false);
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            const string MSGBOX_TITLE = "名前を付けて保存";

            // ファイルダイアログで指定された保存先パスを取得
            editFilePath = saveFileDialog1.FileName;

            try {
                // ファイルの書き込み
                File.WriteAllText(editFilePath, textMemo.Text, Encoding.Default);

                // ファイル名をウィンドウのタイトルに設定
                this.Text = "PlumePad - " + GetFileNameString(editFilePath, '\\');

                // ダーティーフラグのリセット
                setDirty(false);
            }
            catch (Exception ex) {
                // エラー発生の際に Exception の内容を表示
                MessageBox.Show(this, ex.Message, MSGBOX_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void menuExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void textMemo_TextChanged(object sender, EventArgs e)
        {
            setDirty(true);
        }

        private void menuUndo_Click(object sender, EventArgs e)
        {
            // 元に戻せるデータがあるとき
            if(textMemo.CanUndo){
                textMemo.Undo();
                textMemo.ClearUndo();
            }
        }

        private void menuCut_Click(object sender, EventArgs e)
        {
            if(textMemo.SelectedText != ""){
                textMemo.Cut();
            }
        }

        private void menuCopy_Click(object sender, EventArgs e)
        {
            if(textMemo.SelectedText != ""){
                textMemo.Copy();
            }
        }

        private void menuPaste_Click(object sender, EventArgs e)
        {
            // クリップボードの内容がテキストで扱えるのであれば貼り付けを
            if(Clipboard.GetDataObject().GetDataPresent(DataFormats.Text)){
                textMemo.Paste();
            }
        }

        private void menuDelete_Click(object sender, EventArgs e)
        {
            if(textMemo.SelectedText != ""){
                textMemo.Cut();
                Clipboard.Clear();
            }
        }

        private void menuAllSelect_Click(object sender, EventArgs e)
        {
            // 選択されている文字数がテキストボックスの文字数と同じとき
            if(textMemo.SelectionLength == textMemo.Text.Length){
                // テキストボックスの文字列全選択を解除する
                textMemo.SelectionLength = 0;
            }
            else{
                // それ以外のときはテキストの前選択をおこなう
                textMemo.SelectAll();
            }
        }

        private void menuWordWrap_Click(object sender, EventArgs e)
        {
            // 右端に折り返すのチェックが入っているとき
            if(menuWordWrap.Checked){
                menuWordWrap.Checked = false;
                textMemo.WordWrap = false;
            }
            else{
                menuWordWrap.Checked = true;
                textMemo.WordWrap = true;
            }
        }

        private void menuFind_Click(object sender, EventArgs e)
        {
            // 二重起動を防止
            if (findDlg == null || findDlg.IsDisposed) {
                // 検索ダイアログボックス用フォームのインスタンスを生成
                findDlg = new findDialog(dialogMode.Find, textMemo);
                // 検索ダイアログボックスを表示
                findDlg.Show(this);
            }
        }

        private void menuReplace_Click(object sender, EventArgs e)
        {
            // 二重起動を防止
            if (findDlg == null || findDlg.IsDisposed) {
                // 置換ダイアログボックス用フォームのインスタンスを生成
                findDlg = new findDialog(dialogMode.Replace, textMemo);
                // 置換ダイアログボックスを表示
                findDlg.Show(this);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            const string MSGBOX_TITLE = "アプリケーションの終了";

            if(confirmDestructionText(MSGBOX_TITLE)){
                // Application.Idleを解除する
                Application.Idle -= new EventHandler(Application_Idle);
                this.Dispose();
            }
            else{
                // ウィンドウを閉じるのをやめる
                e.Cancel = true;
            }
        }

        private void menuEdit_Click(object sender, EventArgs e)
        {
            if (textMemo.SelectionLength >= 1) {
                menuCut.Enabled = true;
                menuCopy.Enabled = true;
                menuDelete.Enabled = true;
            } else {
                menuCut.Enabled = false;
                menuCopy.Enabled = false;
                menuDelete.Enabled = false;
            }

            if (textMemo.Text.Length >= 1) {
                menuFind.Enabled = true;
                menuReplace.Enabled = true;
                menuAllSelect.Enabled = true;
            } else {
                menuFind.Enabled = false;
                menuReplace.Enabled = false;
                menuAllSelect.Enabled = false;
            }

            // 元に戻せるかのステータスを渡す
            menuUndo.Enabled = textMemo.CanUndo;

            if (Clipboard.GetDataObject().GetDataPresent(DataFormats.Text)) {
                menuPaste.Enabled = true;
            } else {
                menuPaste.Enabled = false;
            }
        }

        private void textMemo_DragDrop(object sender, DragEventArgs e)
        {
            string[] dlagFilePathArray = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            // 複数のファイルがドラックされた場合、パスが配列として取得されるが、
            // 今回のアプリケーションは TextBox が 1 つしかないため、先頭の
            // ファイルのみを扱う
            string filePath = dlagFilePathArray[0];

            // ファイルがテキスト形式でオープン可能なものかどうか確認
            if (CheckFileType(filePath)) {
                // ファイルの内容を TextBox にロード
                textMemo.Text = File.ReadAllText(filePath, Encoding.Default);

                // フォームのタイトル部分にファイル名を表示
                this.Text = "PlumePad - " + GetFileNameString(filePath, '\\');
                editFilePath = filePath;
                setDirty(false);
            }
        }

        // ドロップファイルがオープン可能なものであるかどうかをチェックする関数
        private bool CheckFileType(string filePath)
        {
            // 読み込みを許可するファイルの拡張子を指定 (app.config に定義した方が本当は便利)
            string[] extnArray = { "txt", "cs", "vb", "htm", "html", "xml", "csv", "js", "vbs", "wsh", "c", "cpp", "h", "php", "pl", "rb" };

            foreach (string extn in extnArray) {
                int dotLen = extn.Length;
                if (extn == filePath.Substring(filePath.Length - dotLen, dotLen)) {
                    return true;
                }
            }
            return false;
        }

        private void textMemo_DragEnter(object sender, DragEventArgs e)
        {
            // ドラッグされたのがファイルであるか確認
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                // ドラッグされたデータを受け取る
                e.Effect = DragDropEffects.All;
            }
            else {
                // ドラッグされたデータを受け取らない
                e.Effect = DragDropEffects.None;
            }
        }

        private void menuEditJumpLine_Click(object sender, EventArgs e)
        {
            if (jumpDlg == null || jumpDlg.IsDisposed) {
                jumpDlg = new jumpDialog(textMemo);
                jumpDlg.ShowDialog(this);
            }
        }

        private void menuFont_Click(object sender, EventArgs e)
        {
            if (fontDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                textMemo.Font = fontDialog1.Font;
                textMemo.ForeColor = fontDialog1.Color;
            }

            fontDialog1.Dispose();
        }

        private void menuDateTimeInsert_Click(object sender, EventArgs e)
        {
            // 日付と時刻の挿入
            textMemo.SelectedText = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
        }

        private void menuHelpAbout_Click(object sender, EventArgs e)
        {
            // バージョン情報を表示する
            AboutDialog AboutForm = new AboutDialog();
            AboutForm.ShowDialog();
        }
    }
}