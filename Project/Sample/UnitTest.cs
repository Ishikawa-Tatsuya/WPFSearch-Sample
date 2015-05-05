using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Codeer.Friendly.Windows;
using System.Diagnostics;
using Codeer.Friendly;
using Codeer.Friendly.Dynamic;
using System.Windows;
using RM.Friendly.WPFStandardControls;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Sample
{
    [TestClass]
    public class UnitTest
    {
        WindowsAppFriend _app;

        [TestInitialize]
        public void TestInitialize()
        {
            _app = new WindowsAppFriend(Process.Start("Target.exe"));
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Process.GetProcessById(_app.ProcessId).CloseMainWindow();
        }

        [TestMethod]
        public void 普通はこんな感じで書く()
        {
            AppVar main = _app.Type<Application>().Current.MainWindow;
            var logicalTree = main.LogicalTree();

            var textBox = new WPFTextBox(logicalTree.ByBinding("Memo").ByType<TextBox>().Single());
            var textBlock = new WPFTextBlock(logicalTree.ByBinding("Memo").ByType<TextBlock>().Single());
            var button = new WPFButtonBase(logicalTree.ByBinding("CommandOK").Single());
            var listBox = new WPFListBox(logicalTree.ByBinding("Persons").Single());
        }

        [TestMethod]
        public void Bindingと型から検索()
        {
            AppVar main = _app.Type<Application>().Current.MainWindow;

            //LogicalTreeをフラットなコレクションにする
            //VisualTreeもある
            //IWPFDependencyObjectCollection<DependencyObject>という型が返ってきている
            var logicalTree = main.LogicalTree();

            //Bindingのパスから取得
            var commands = logicalTree.ByBinding("CommandOK");

            //一つに確定している場合はSingleを使う
            AppVar button = commands.Single();

            //どのデータへのBindingかも指定できる
            button = logicalTree.ByBinding("CommandOK", new ExplicitAppVar(main.Dynamic().DataContext)).Single();

            //複数個の場合はこの後さらに絞り込みが必要
            var memos = logicalTree.ByBinding("Memo");

            //この場合はタイプから特定できる
            var textBox = memos.ByType<TextBox>().Single();
        }

        [TestMethod]
        public void Genericを使った型検索は戻り値に型情報が残っている()
        {
            AppVar main = _app.Type<Application>().Current.MainWindow;

            //将来的に型固有の拡張メソッドを用意するかも
            IWPFDependencyObjectCollection<TextBox> collection = main.LogicalTree().ByBinding("Memo").ByType<TextBox>();
        }

        [TestMethod]
        public void ListBoxとか()
        {
            AppVar main = _app.Type<Application>().Current.MainWindow;
            var logicalTree = main.LogicalTree();
            var listBox = new WPFListBox(logicalTree.ByBinding("Persons").Single());

            //アイテム取得
            //可視状態にしている
            var item = listBox.GetItem(20);

            //NameにバインドされたTextBlockを取得する
            //ListBoxのアイテムはLogicalTree上には現れない
            var textBlock = new WPFTextBlock(item.VisualTree().ByBinding("Name").Single());
            Assert.AreEqual("U", textBlock.Text);
        }

        [TestMethod]
        public void Ancestors()
        {
            AppVar main = _app.Type<Application>().Current.MainWindow;
            var buttonOK = main.LogicalTree().ByBinding("CommandOK").Single();

            //逆向きにもたどれる
            var tree = buttonOK.LogicalTree(TreeRunDirection.Ancestors);

            //最初のボタンが入っていて、最後がトップレベルウィンドウ
            Assert.AreEqual(buttonOK, tree[0]);
            Assert.AreEqual(main, tree[tree.Count - 1]);
        }

        [TestMethod]
        public void 内部からの取得もサポート()
        {
            //内部で処理をするための準備
            WPFStandardControls_3_5.Injection(_app);
            WindowsAppExpander.LoadAssembly(_app, GetType().Assembly);

            //相手プロセスで取得ロジック実行
            var layout = _app.Type(GetType()).GetLayout(_app.Type<Application>().Current.MainWindow);

            //戻り値に格納されているのでそれを使う
            var textBox = new WPFTextBox(layout.TextBox);
            var textBlock = new WPFTextBlock(layout.TextBlock);
            var button = new WPFButtonBase(layout.Button);
            var listBox = new WPFListBox(layout.ListBox);
        }

        class Layout
        {
            public TextBox TextBox { get; set; }
            public TextBlock TextBlock { get; set; }
            public Button Button { get; set; }
            public ListBox ListBox { get; set; }
        }

        static Layout GetLayout(Window main)
        {
            //通常のプログラムなんで、普通のLinqとかも使えるし、
            //ご自由にどうぞ
            var logicalTree = main.LogicalTree();
            return new Layout()
            {
                TextBox = (TextBox)logicalTree.ByBinding("Memo").ByType<TextBox>().Single(),
                TextBlock = (TextBlock)logicalTree.ByBinding("Memo").ByType<TextBlock>().Single(),
                Button = (Button)logicalTree.ByType<Button>().Single(),
                ListBox = (ListBox)logicalTree.ByBinding("Persons").Single()
            };
        }
    }
}
